

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 7.00.0555 */
/* at Wed Apr 04 20:36:29 2012
 */
/* Compiler settings for ComTest.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 7.00.0555 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __ComTest_i_h__
#define __ComTest_i_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IPeachComTest_FWD_DEFINED__
#define __IPeachComTest_FWD_DEFINED__
typedef interface IPeachComTest IPeachComTest;
#endif 	/* __IPeachComTest_FWD_DEFINED__ */


#ifndef __PeachComTest_FWD_DEFINED__
#define __PeachComTest_FWD_DEFINED__

#ifdef __cplusplus
typedef class PeachComTest PeachComTest;
#else
typedef struct PeachComTest PeachComTest;
#endif /* __cplusplus */

#endif 	/* __PeachComTest_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IPeachComTest_INTERFACE_DEFINED__
#define __IPeachComTest_INTERFACE_DEFINED__

/* interface IPeachComTest */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IPeachComTest;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("F4FCA5E2-BBC4-409D-B50C-7F1D116F0ED9")
    IPeachComTest : public IDispatch
    {
    public:
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Method1( 
            /* [in] */ BSTR str,
            /* [retval][out] */ BSTR *ret) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Method2( 
            /* [retval][out] */ BSTR *ret) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Method3( 
            /* [in] */ BSTR str) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Method4( void) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_Property1( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_Property1( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Method5( 
            /* [in] */ LONG int1,
            /* [in] */ SHORT short1,
            /* [retval][out] */ LONG *retval) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Method6( 
            /* [in] */ SHORT shortParam,
            /* [in] */ INT intParam) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IPeachComTestVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IPeachComTest * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IPeachComTest * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IPeachComTest * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IPeachComTest * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IPeachComTest * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IPeachComTest * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IPeachComTest * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Method1 )( 
            IPeachComTest * This,
            /* [in] */ BSTR str,
            /* [retval][out] */ BSTR *ret);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Method2 )( 
            IPeachComTest * This,
            /* [retval][out] */ BSTR *ret);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Method3 )( 
            IPeachComTest * This,
            /* [in] */ BSTR str);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Method4 )( 
            IPeachComTest * This);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_Property1 )( 
            IPeachComTest * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_Property1 )( 
            IPeachComTest * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Method5 )( 
            IPeachComTest * This,
            /* [in] */ LONG int1,
            /* [in] */ SHORT short1,
            /* [retval][out] */ LONG *retval);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Method6 )( 
            IPeachComTest * This,
            /* [in] */ SHORT shortParam,
            /* [in] */ INT intParam);
        
        END_INTERFACE
    } IPeachComTestVtbl;

    interface IPeachComTest
    {
        CONST_VTBL struct IPeachComTestVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IPeachComTest_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IPeachComTest_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IPeachComTest_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IPeachComTest_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define IPeachComTest_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define IPeachComTest_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define IPeachComTest_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define IPeachComTest_Method1(This,str,ret)	\
    ( (This)->lpVtbl -> Method1(This,str,ret) ) 

#define IPeachComTest_Method2(This,ret)	\
    ( (This)->lpVtbl -> Method2(This,ret) ) 

#define IPeachComTest_Method3(This,str)	\
    ( (This)->lpVtbl -> Method3(This,str) ) 

#define IPeachComTest_Method4(This)	\
    ( (This)->lpVtbl -> Method4(This) ) 

#define IPeachComTest_get_Property1(This,pVal)	\
    ( (This)->lpVtbl -> get_Property1(This,pVal) ) 

#define IPeachComTest_put_Property1(This,newVal)	\
    ( (This)->lpVtbl -> put_Property1(This,newVal) ) 

#define IPeachComTest_Method5(This,int1,short1,retval)	\
    ( (This)->lpVtbl -> Method5(This,int1,short1,retval) ) 

#define IPeachComTest_Method6(This,shortParam,intParam)	\
    ( (This)->lpVtbl -> Method6(This,shortParam,intParam) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IPeachComTest_INTERFACE_DEFINED__ */



#ifndef __ComTestLib_LIBRARY_DEFINED__
#define __ComTestLib_LIBRARY_DEFINED__

/* library ComTestLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_ComTestLib;

EXTERN_C const CLSID CLSID_PeachComTest;

#ifdef __cplusplus

class DECLSPEC_UUID("8FAEEEE1-AAA5-4B77-8CBA-BFDCE3E3C7E8")
PeachComTest;
#endif
#endif /* __ComTestLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


