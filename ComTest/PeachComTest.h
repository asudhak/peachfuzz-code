// PeachComTest.h : Declaration of the CPeachComTest

#pragma once
#include "resource.h"       // main symbols

#include "ComTest_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CPeachComTest

class ATL_NO_VTABLE CPeachComTest :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CPeachComTest, &CLSID_PeachComTest>,
	public IDispatchImpl<IPeachComTest, &IID_IPeachComTest, &LIBID_ComTestLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CPeachComTest()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_PEACHCOMTEST)


BEGIN_COM_MAP(CPeachComTest)
	COM_INTERFACE_ENTRY(IPeachComTest)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:

	STDMETHOD(Method1)(BSTR str, BSTR* ret);
	STDMETHOD(Method2)(BSTR* ret);
	STDMETHOD(Method3)(BSTR str);
	STDMETHOD(Method4)(void);
	STDMETHOD(get_Property1)(BSTR* pVal);
	STDMETHOD(put_Property1)(BSTR newVal);
	STDMETHOD(Method5)(LONG int1, SHORT short1, LONG* retval);
	STDMETHOD(Method6)(SHORT shortParam, INT intParam);
};

OBJECT_ENTRY_AUTO(__uuidof(PeachComTest), CPeachComTest)
