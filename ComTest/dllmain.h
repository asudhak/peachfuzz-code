// dllmain.h : Declaration of module class.

class CComTestModule : public CAtlDllModuleT< CComTestModule >
{
public :
	DECLARE_LIBID(LIBID_ComTestLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_COMTEST, "{34FEBB76-70F7-427E-AC4E-B5D0E74600E4}")
};

extern class CComTestModule _AtlModule;
