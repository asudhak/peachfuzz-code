using System;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Collections.Concurrent;
using System.Linq.Expressions;

using CloneFunc = System.Func<System.Collections.Hashtable, object, object, object>;
using ParamList = System.Collections.Generic.List<System.Linq.Expressions.ParameterExpression>;
using ExprList = System.Collections.Generic.List<System.Linq.Expressions.Expression>;


namespace Peach.Core
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class OnCloningAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class OnClonedAttribute : Attribute
	{
	}

	public class ObjectCopier
	{
		#region Static Expression Tree Caching

		static ConcurrentDictionary<Type, CloneFunc> cloners = new ConcurrentDictionary<Type, CloneFunc>();

		static CloneFunc findOrCreateCloner(Type type)
		{
			CloneFunc cloner;

			if (!cloners.TryGetValue(type, out cloner))
			{
				cloner = new ObjectCopier(type).result;
				cloners.TryAdd(type, cloner);
			}

			return cloner;
		}

		#endregion

		#region Public Clone Functions

		public static T Clone<T>(T obj)
		{
			return Clone(obj, null);
		}

		public static T Clone<T>(T obj, object ctx)
		{
			if (obj == null)
				return default(T);

			var table = new Hashtable();
			var cloner = findOrCreateCloner(obj.GetType());

			return (T)cloner(table, obj, ctx);
		}

		#endregion

		#region Private Instance Members

		// The source object to be cloned
		ParameterExpression obj = Expression.Parameter(typeof(object), "obj");
		// Context object for OnCloning / OnCloned methods
		ParameterExpression ctx = Expression.Parameter(typeof(object), "ctx");
		// Hashtable to prevent circular dependencies
		ParameterExpression tbl = Expression.Parameter(typeof(Hashtable), "tbl");
		// The final compiled clone expression tree
		CloneFunc result;

		#endregion

		#region Static Clone Helpers

		/// <summary>
		/// Downcast obj to type and return typed instance
		/// </summary>
		/// <param name="type">Destination type</param>
		/// <param name="original">Variable to cast</param>
		/// <param name="vars">List of expression variables</param>
		/// <param name="exprs">List of expressions</param>
		/// <returns>Variable of <paramref name="type"/></returns>
		static Expression Downcast(Type type, Expression original, ParamList vars, ExprList exprs)
		{
			var typed = Expression.Variable(type);

			vars.Add(typed);

			exprs.Add(
				Expression.Assign(
					typed,
					Expression.Convert(
						original,
						type
					)
				)
			);

			return typed;
		}

		/// <summary>
		/// Returns all methods on type <paramref name="t"/> that are decorated
		/// with an attribute of type <paramref name="attribute"/>
		/// </summary>
		/// <param name="attribute">Type of attribute to match</param>
		/// <param name="t">Type to search</param>
		/// <returns>List of methods</returns>
		static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type attribute, Type t)
		{
			var methods = new List<MethodInfo>();

			for (var type = t; (type != null) && (type != typeof(object)); type = type.BaseType)
			{
				foreach (var info in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					if (info.IsDefined(attribute, false))
					{
						methods.Add(info);
					}
				}
			}

			methods.Reverse();

			return methods;
		}

		/// <summary>
		/// Returns all serializable fields on type <paramref name="t"/> that
		/// need to be transferred when cloning.
		/// </summary>
		/// <param name="t">Type to search</param>
		/// <returns>List of fields</returns>
		static IEnumerable<FieldInfo> GetFields(Type t)
		{
			var fields = new Dictionary<string, FieldInfo>();

			for (var type = t; (type != null) && (type != typeof(object)); type = type.BaseType)
			{
				foreach (var info in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (!info.Attributes.HasFlag(FieldAttributes.NotSerialized))
					{
						var fullName = info.DeclaringType + "." + info.Name;

						if (!fields.ContainsKey(fullName))
						{
							fields.Add(fullName, info);
						}
					}
				}
			}

			return fields.Values;
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Construct object cloner expression tree
		/// </summary>
		/// <param name="type">Type to compute</param>
		ObjectCopier(Type type)
		{
			ExprList exprs = new ExprList();
			ParamList vars = new ParamList();

			Expression clone;
			Expression final;

			if (type.IsPrimitive || type == typeof(string))
			{
				clone = obj;
			}
			else if (type.IsArray)
			{
				var elemType = type.GetElementType();

				if (elemType.IsPrimitive || elemType == typeof(string))
				{
					clone = ClonePrimitiveArray(type, obj);
				}
				else
				{
					// Need to downcast prior to cloning array
					var typed = Downcast(type, obj, vars, exprs);
					clone = CloneComplexArray(type, typed, vars, exprs);
				}
			}
			else
			{
				clone = CloneComplexType(type, vars, exprs);
			}

			exprs.Add(clone);

			// An expression list needs to be wrapped in a block
			if ((exprs.Count == 1) && (vars.Count == 0))
				final = exprs[0];
			else
				final = Expression.Block(vars, exprs);

			// Value types require manual boxing
			if (type.IsValueType)
				final = Expression.Convert(final, typeof(object));

			// Compile the final expression
			result = Expression.Lambda<CloneFunc>(final, tbl, obj, ctx).Compile();
		}

		#endregion

		#region Clone Members

		Expression CloneComplexType(Type type, ParamList vars, ExprList exprs)
		{
			/*
			 * type clone = table[obj];
			 * 
			 * if (clone == null)
			 * {
			 *   if (!obj.OnCloning(ctx))
			 *   {
			 *     clone = obj;
			 *     table[obj] = obj;
			 *   }
			 *   else
			 *   {
			 *     clone = GetUninitializedObject();
			 *     table[obj] = clone;
			 *     // Copy Fields
			 *     clone.OnCloned(ctx);
			 *   }
			 * }
			 * 
			 * return clone;
			 */

			// Get functions decorated with callback attributes
			var onCloning = GetMethodsWithAttribute(typeof(OnCloningAttribute), type);
			var onCloned = GetMethodsWithAttribute(typeof(OnClonedAttribute), type);

			// Get functions we need to call
			var getUninitializedObject = typeof(FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Static | BindingFlags.Public);
			var getItem = typeof(Hashtable).GetMethod("get_Item");
			var setItem = typeof(Hashtable).GetMethod("set_Item");

			// Alloc variable to store clone in
			ParameterExpression clone = Expression.Variable(type);
			vars.Add(clone);

			// Start by downcasting obj to i'ts concrete type
			var typed = Downcast(type, obj, vars, exprs);

			// Build expression list for actually copying the object
			ExprList copy = new ExprList() {

				// clone = (tyoe)FormatterServices.GetUninitializedObject(type)
				Expression.Assign(
					clone,
					Expression.Convert(
						Expression.Call(
							getUninitializedObject,
							Expression.Constant(type)
						),
					type)
				),

				// tbl[obj] = clone
				Expression.Call(
					tbl,
					setItem,
					obj,
					clone
				),
			};

			// Transfer all feilds
			CopyComplexType(type, typed, clone, copy);

			// Call OnCloned functions
			foreach (var item in onCloned)
			{
				copy.Add(Expression.Call(clone, item, typed, ctx));
			}

			// Compile 'copy object' into expression block
			Expression expr = Expression.Block(copy);

			// Filter 'copy object' based on OnCloning functions
			foreach (var item in onCloning)
			{
				//if (obj.OnCloning(ctx)) { copyObject(); } else { clone = obj; tbl[obj] = clone; }
				expr = Expression.IfThenElse(
					Expression.Call(typed, item, ctx),
					expr,
					Expression.Block(
						Expression.Assign(clone, typed),
						Expression.Call(tbl, setItem, obj, clone)
					)
				);
			}

			// Load clone from Hashtable of visited objects
			exprs.Add(
				Expression.Assign(
					clone,
					Expression.Convert(
						Expression.Call(tbl, getItem, obj),
						type
					)
				)
			);

			// Filter obj.OnCloning() test against Hashtable of visited objects
			exprs.Add(
				Expression.IfThen(
					Expression.Equal(clone, Expression.Constant(null)),
					expr
				)
			);

			return clone;
		}

		Expression ClonePrimitiveArray(Type type, Expression original)
		{
			/*
			 * return (type)((Array)original).Clone();
			 */

			var arrayClone = typeof(Array).GetMethod("Clone");

			var clone = Expression.Convert(
				Expression.Call(
					Expression.Convert(
						original,
						typeof(Array)
					),
					arrayClone
				),
				type
			);

			return clone;
		}

		Expression CloneComplexArray(Type type, Expression original, ParamList vars, ExprList exprs)
		{
			// Create temporary variable to store the clone in
			var clone = Expression.Variable(type);
			vars.Add(clone);

			var lengths = new ParamList();
			var indexes = new ParamList();
			var labels = new List<LabelTarget>();

			var rank = type.GetArrayRank();
			var elementType = type.GetElementType();

			// Get the length of each dimension in the array
			MethodInfo getLength = typeof(Array).GetMethod("GetLength");
			for (int index = 0; index < rank; ++index)
			{
				// Set up a variable to track the dimension
				var dim = Expression.Variable(typeof(int));
				indexes.Add(dim);

				// Set up label to stop the clone loop for this dimension
				labels.Add(Expression.Label());

				// Set up variable to track the length
				var len = Expression.Variable(typeof(int));
				lengths.Add(len);

				/*
				 * lengths[i] = original.GetLength(i)
				 */

				exprs.Add(
					Expression.Assign(
						len,
						Expression.Call(
							original,
							getLength,
							Expression.Constant(index)
						)
					)
				);

				vars.Add(len);
				vars.Add(dim);
			}

			// Make a new array to copy the elements into
			exprs.Add(
				Expression.Assign(
					clone,
					Expression.NewArrayBounds(elementType, lengths)
				)
			);

			// Initialize the index for the outter loop.
			exprs.Add(Expression.Assign(indexes[0], Expression.Constant(0)));

			// Build loop for copying each dimension of the array
			Expression loop = null;

			for (int i = rank - 1; i >= 0; --i)
			{
				var loopVars = new ParamList();
				var loopExpr = new ExprList();

				// If this dimension has already copied, there is nothing to do
				loopExpr.Add(
					Expression.IfThen(
						Expression.GreaterThanOrEqual(indexes[i], lengths[i]),
						Expression.Break(labels[i])
					)
				);

				if (loop == null)
				{
					// Build the array element cloning loop
					if (elementType.IsPrimitive || (elementType == typeof(string)))
					{
						// Straight assignment
						loopExpr.Add(
							Expression.Assign(
								Expression.ArrayAccess(clone, indexes),
								Expression.ArrayAccess(original, indexes)
							)
						);
					}
					else if (elementType.IsValueType)
					{
						// Value types are cloned by assigning the fields
						CopyComplexType(
							elementType,
							Expression.ArrayAccess(original, indexes),
							Expression.ArrayAccess(clone, indexes),
							loopExpr
						);
					}
					else
					{
						// Clone each reference recursively
						// First we need the single element
						var elem = Expression.Variable(elementType);
						loopVars.Add(elem);

						loopExpr.Add(Expression.Assign(elem, Expression.ArrayAccess(original, indexes)));

						var innerVars = new ParamList();
						var innerExpr = new ExprList();

						// If the nested type is an array, just create the array in place
						if (elementType.IsArray)
						{
							Expression tmp;

							Type nestedType = elementType.GetElementType();
							if (nestedType.IsPrimitive || nestedType == typeof(string))
							{
								tmp = ClonePrimitiveArray(elementType, elem);
							}
							else
							{
								tmp = CloneComplexArray(elementType, elem, innerVars, innerExpr);
							}

							// Set the cloned element to the index in our cloned array
							innerExpr.Add(Expression.Assign(Expression.ArrayAccess(clone, indexes), tmp));
						}
						else
						{
							// Find the complex type cloner and recurse!
							var findOrCreateCloner = typeof(ObjectCopier).GetMethod("findOrCreateCloner", BindingFlags.NonPublic | BindingFlags.Static);
							var getType = typeof(object).GetMethod("GetType");
							var invoke = typeof(CloneFunc).GetMethod("Invoke");

							/*
							 * clone[indices] = findOrCreateCloner(elem.GetType()).Invoke(tbl, elem, ctx));
							 */

							innerExpr.Add(
								Expression.Assign(
									Expression.ArrayAccess(clone, indexes),
										Expression.Convert(
											Expression.Call(
												Expression.Call(
													findOrCreateCloner,
													Expression.Call(elem, getType)
												),
												invoke,
												tbl,
												elem,
												ctx
											),
										elementType
									)
								)
							);
						}

						// Must check each element for null before running the inner expressions.
						loopExpr.Add(
							Expression.IfThen(
								Expression.NotEqual(elem, Expression.Constant(null)),
								Expression.Block(innerVars, innerExpr)
							)
						);
					}
				}
				else
				{
					// Reset index used by inner loop and run loop again
					loopExpr.Add(Expression.Assign(indexes[i + 1], Expression.Constant(0)));
					loopExpr.Add(loop);
				}

				// Increment the index of the current dimension we are cloning
				loopExpr.Add(Expression.PreIncrementAssign(indexes[i]));

				// Create a new loop for the next dimension
				loop = Expression.Loop(Expression.Block(loopVars, loopExpr), labels[i]);
			}

			// Add all the nested loops to the clone expression list
			exprs.Add(loop);

			return clone;
		}

		#endregion

		#region Copy Fields Methods

		/// <summary>
		/// Generates expression list to copy all fields in a complex object.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="original">Source object of type <paramref name="type"/></param>
		/// <param name="clone">Destination object of type <paramref name="type"/></param>
		/// <param name="exprs">Expression list to populate</param>
		void CopyComplexType(Type type, Expression original, Expression clone, ExprList exprs)
		{
			foreach (var fieldInfo in GetFields(type))
			{
				Type fieldType = fieldInfo.FieldType;

				if (fieldType.IsPrimitive || (fieldType == typeof(string)))
				{
					// Directly assign primitives
					// Need to properly handle IsInitOnly
					exprs.Add(AssignField(fieldInfo, clone, Expression.Field(original, fieldInfo)));
				}
				else if (fieldType.IsValueType)
				{
					// Directly assign all members inside value types
					// Don't need to worry about IsInitOnly because of IsValueType
					CopyComplexType(
						fieldType, 
						Expression.Field(original, fieldInfo),
						Expression.Field(clone, fieldInfo),
						exprs
					);
				}
				else
				{
					// Invoke cloner for reference types
					CopyRefType(fieldInfo, original, clone, exprs);
				}
			}
		}

		/// <summary>
		/// Generates expression list to copy a single field in a complex object.
		/// </summary>
		/// <param name="fieldInfo">Field to copy</param>
		/// <param name="original">Source object of type <paramref name="type"/></param>
		/// <param name="clone">Destination object of type <paramref name="type"/></param>
		/// <param name="exprs">Expression list to populate</param>
		void CopyRefType(FieldInfo fieldInfo, Expression original, Expression clone, ExprList exprs)
		{
			var fieldExprs = new ExprList();
			var fieldVars = new ParamList();
			var fieldType = fieldInfo.FieldType;

			if (fieldType.IsArray)
			{
				var elementType = fieldType.GetElementType();

				Expression tmp;

				if (elementType.IsPrimitive || elementType == typeof(string))
					tmp = ClonePrimitiveArray(fieldType, Expression.Field(original, fieldInfo));
				else
					tmp = CloneComplexArray(fieldType, Expression.Field(original, fieldInfo), fieldVars, fieldExprs);

				// Assign 'tmp' to clone.fieldInfo
				// Need to properly handle IsInitOnly
				fieldExprs.Add(AssignField(fieldInfo, clone, tmp));
			}
			else
			{
				// Find the complex type cloner and recurse!
				// Need to properly handle IsInitOnly
				var findOrCreateCloner = typeof(ObjectCopier).GetMethod("findOrCreateCloner", BindingFlags.NonPublic | BindingFlags.Static);
				var getType = typeof(object).GetMethod("GetType");
				var invoke = typeof(CloneFunc).GetMethod("Invoke");

				/*
				 * clone.field = findOrCreateCloner(original.field.GetType()).Invoke(tbl, original.field, ctx));
				 */

				fieldExprs.Add(
					AssignField(
						fieldInfo,
						clone,
						Expression.Convert(
							Expression.Call(
								Expression.Call(
									findOrCreateCloner,
									Expression.Call(
										Expression.Field(original, fieldInfo),
										getType
									)
								),
								invoke,
								tbl,
								Expression.Field(original, fieldInfo),
								ctx
							),
							fieldType
						)
					)
				);
			}

			// Guard against null and then call fieldExprs
			exprs.Add(
				Expression.IfThen(
					Expression.NotEqual(
						Expression.Field(original, fieldInfo),
						Expression.Constant(null)
					),
					Expression.Block(
						fieldVars,
						fieldExprs
					)
				)
			);
		}

		Expression AssignField(FieldInfo fieldInfo, Expression clone, Expression value)
		{
			if (!fieldInfo.IsInitOnly)
				return Expression.Assign(Expression.Field(clone, fieldInfo), value);

			// For IsInitOnly fields, we need to use reflection to set the value
			var getType = typeof(object).GetMethod("GetType");
			var getField = typeof(Type).GetMethod("GetField", new [] { typeof(string) , typeof(BindingFlags) });
			var setValue = typeof(FieldInfo).GetMethod("SetValue", new [] { typeof(object), typeof(object) });

			/*
			 * clone.GetType().GetField(fieldInfo.Name).SetValue(value)
			 */

			var expr = Expression.Call(
				Expression.Call(
					Expression.Call(clone, getType),
					getField,
					Expression.Constant(fieldInfo.Name),
					Expression.Constant(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				),
				setValue,
				clone,
				value
			);

			return expr;
		}

		#endregion
	}
}
