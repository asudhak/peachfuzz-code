// PeachComTest.cpp : Implementation of CPeachComTest

#include "stdafx.h"
#include "PeachComTest.h"


// CPeachComTest


STDMETHODIMP CPeachComTest::Method1(BSTR str, BSTR* ret)
{
	// TODO: Add your implementation code here

	wprintf(L"CPeachComTest::Method1(%s)\n", str);

	*ret = SysAllocString(L"Method1Return");

	return S_OK;
}

STDMETHODIMP CPeachComTest::Method2(BSTR* ret)
{
	// TODO: Add your implementation code here
	printf("CPeachComTest::Method2()\n");
	*ret = SysAllocString(L"Method2Return");

	return S_OK;
}

STDMETHODIMP CPeachComTest::Method3(BSTR str)
{
	// TODO: Add your implementation code here
	wprintf(L"CPeachComTest::Method3(%s)\n", str);

	return S_OK;
}

STDMETHODIMP CPeachComTest::Method4(void)
{
	// TODO: Add your implementation code here
	printf("CPeachComTest::Method4()\n");

	return S_OK;
}

BSTR property1;

STDMETHODIMP CPeachComTest::get_Property1(BSTR* pVal)
{
	// TODO: Add your implementation code here

	//pVal = &property1;

	printf("CPeachComTest::get_Property1()\n");

	return S_OK;
}

STDMETHODIMP CPeachComTest::put_Property1(BSTR newVal)
{
	// TODO: Add your implementation code here

	wprintf(L"CPeachComTest::put_Property1(%s)\n", newVal);

	return S_OK;
}

STDMETHODIMP CPeachComTest::Method5(LONG int1, SHORT short1, LONG* retval)
{
	// TODO: Add your implementation code here
	*retval = int1 + short1;
	wprintf(L"CPeachComTest::Method5(%d, %d, %d)\n", int1, short1, *retval);

	return S_OK;
}

STDMETHODIMP CPeachComTest::Method6(SHORT shortParam, INT intParam)
{
	// TODO: Add your implementation code here
	wprintf(L"CPeachComTest::Method6(%d, %d)\n", shortParam, intParam);

	return S_OK;
}
