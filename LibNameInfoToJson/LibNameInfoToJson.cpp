// LibNameInfoToJson.cpp: 定义控制台应用程序的入口点。
//
#include "stdafx.h"
#include "3rd-party/elib/lib2.h"
#include "3rd-party/elib/lang.h"
#include "3rd-party/rapidjson/writer.h"
#include <iostream>
using namespace rapidjson;

template<typename OutputStream, typename TargetEncoding, typename StackAllocator, unsigned writeFlags>
void AnsiString(Writer<OutputStream, UTF16LE<>, TargetEncoding, StackAllocator, writeFlags> &writer, LPSTR str, int codepage) {
	if (str == nullptr)
	{
		writer.Null();
		return;
	}
	int size = MultiByteToWideChar(codepage, 0, str, -1, nullptr, 0);
	auto wstr = new wchar_t[size];
	MultiByteToWideChar(codepage, 0, str, -1, wstr, size);
	writer.String(wstr, size - 1);
	delete wstr;
}
int main()
{
	setlocale(LC_CTYPE, "");

	int nArgs;
	auto szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
	if (nullptr == szArglist) return 1;
	auto fneName = szArglist[1];
	HANDLE hFile = 0;

	switch (nArgs)
	{
	case 2:
		break;
	case 3:
		hFile=CreateFileW(szArglist[2], GENERIC_WRITE, 0, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
		break;
	default:
		LocalFree(szArglist);
		return 1;
	}

	auto fne = LoadLibraryW(fneName);
	if (nullptr == fne)
	{
		wchar_t fnePath[MAX_PATH];
		HKEY hkey;
		if (RegOpenKeyExW(HKEY_CURRENT_USER, L"Software\\FlySky\\E\\Install", 0, KEY_READ, &hkey) == ERROR_SUCCESS)
		{
			DWORD len = sizeof(fnePath);
			if (RegQueryValueExW(hkey, L"Path", nullptr, nullptr, (LPBYTE)fnePath, &len) == ERROR_SUCCESS)
			{
				len = len / sizeof(wchar_t) - 1;
				if (fnePath[len - 1] != '\\')
				{
					wcscat(fnePath, L"\\");
					len += 1;
				}
				auto len_fneName = wcslen(fneName);
				memcpy(fnePath + len, fneName, len_fneName * sizeof(wchar_t) + sizeof(wchar_t));
				if(wcschr(fneName,L'.')==nullptr) 
					wcscat(fnePath, L".fne");
				fne = LoadLibraryW(fnePath);

			}
			RegCloseKey(hkey);
		}
	}
	if (nullptr == fne) return 1;
	LocalFree(szArglist);
	auto getNewInf = (PFN_GET_LIB_INFO)GetProcAddress(fne, FUNCNAME_GET_LIB_INFO);
	if (nullptr == getNewInf) return 1;
	auto pLibInfo = getNewInf();
	if (pLibInfo->m_dwState & LBS_NO_EDIT_INFO) return 1;
	int codepage;
	switch (pLibInfo->m_nLanguage)
	{
	case __GBK_LANG_VER:
		if (IsValidCodePage(54936))
			codepage = 54936; //GB18030
		else
			codepage = 936; //GBK
		break;
	case __BIG5_LANG_VER:
		codepage = 950; //Big5
		break;
	case __SJIS_JP_LANG_VER:
		codepage = 932; //Shift-JIS
		break;
	case __ENGLISH_LANG_VER:
		codepage = 1252; //ASCII
		break;
	default:
		codepage = 0;
		break;
	}

	GenericStringBuffer<UTF16LE<> >  s;
	Writer<GenericStringBuffer<UTF16LE<> >, UTF16LE<>, UTF16LE<> > writer(s);

	writer.StartObject();
	writer.Key(L"Guid");
	AnsiString(writer, pLibInfo->m_szGuid, codepage);
	writer.Key(L"Name");
	AnsiString(writer, pLibInfo->m_szName, codepage);
	writer.Key(L"VersionName");
	{
		wchar_t versionStr[256];
		wsprintfW(versionStr,L"%d.%d", pLibInfo->m_nMajorVersion,pLibInfo->m_nMinorVersion);
		writer.String(versionStr);
	}
	writer.Key(L"VersionCode");
	writer.Int(pLibInfo->m_nBuildNumber);
	writer.Key(L"DataType");
	writer.StartArray();
	for (int i = 0; i < pLibInfo->m_nDataTypeCount; i++)
	{
		auto &type = pLibInfo->m_pDataType[i];
		writer.StartObject();
		writer.Key(L"Name");
		AnsiString(writer, type.m_szName, codepage);
		writer.Key(L"EnglshName");
		AnsiString(writer, type.m_szEgName, codepage);

		if (type.m_dwState & LDT_ENUM)
		{
			writer.Key(L"IsEnum");
			writer.Bool(true);
		}

		writer.Key(L"Evnet");
		writer.StartArray();
		uint8_t* pEvent = (uint8_t*)type.m_pEventBegin;
		for (int j = 0; j < type.m_nEventCount; j++)
		{
			auto &event = *(EVENT_INFO*)pEvent;
			writer.StartObject();
			writer.Key(L"Name");
			AnsiString(writer, event.m_szName, codepage);
			writer.EndObject();
			pEvent += event.m_dwState & EV_IS_VER2 ? sizeof(EVENT_INFO2) : sizeof(EVENT_INFO);
		}
		writer.EndArray();

		writer.Key(L"Member");
		writer.StartArray();
		if (type.m_nPropertyCount != 0)
		{
			for (int j = 0; j < type.m_nPropertyCount; j++)
			{
				auto &prop = type.m_pPropertyBegin[j];
				writer.StartObject();
				writer.Key(L"Name");
				AnsiString(writer, prop.m_szName, codepage);
				writer.Key(L"EnglishName");
				AnsiString(writer, prop.m_szEgName, codepage);
				writer.EndObject();
			}
		}
		else
		{
			for (int j = 0; j < type.m_nElementCount; j++)
			{
				auto &member = type.m_pElementBegin[j];
				writer.StartObject();
				writer.Key(L"Name");
				AnsiString(writer, member.m_szName, codepage);
				writer.Key(L"EnglishName");
				AnsiString(writer, member.m_szEgName, codepage);
				if (member.m_dwState & LES_HAS_DEFAULT_VALUE)
				{
					switch (member.m_dtType)
					{
					case SDT_BYTE:
					case SDT_SHORT:
					case SDT_INT:
					case SDT_INT64:
						writer.Key(L"Default");
						writer.Int(member.m_nDefault);
						break;
					case SDT_FLOAT:
					case SDT_DOUBLE:
						writer.Key(L"Default");
						writer.Double((double)member.m_nDefault);
						break;
					case SDT_BOOL:
						writer.Key(L"Default");
						writer.Bool(member.m_nDefault != 0);
						break;
					case SDT_TEXT:
						writer.Key(L"Default");
						AnsiString(writer, (LPSTR)member.m_nDefault, codepage);
						break;
					default:
						break;
					}
				}
				writer.EndObject();
			}
		}
		writer.EndArray();

		writer.Key(L"Method");
		writer.StartArray();
		for (int j = 0; j < type.m_nCmdCount; j++)
		{
			writer.Int(type.m_pnCmdsIndex[j]);
		}
		writer.EndArray();

		writer.EndObject();
	}
	writer.EndArray();
	writer.Key(L"Cmd");
	writer.StartArray();
	for (int i = 0; i < pLibInfo->m_nCmdCount; i++)
	{
		auto &cmd = pLibInfo->m_pBeginCmdInfo[i];
		writer.StartObject();
		writer.Key(L"Name");
		AnsiString(writer, cmd.m_szName, codepage);
		writer.Key(L"EnglshName");
		AnsiString(writer, cmd.m_szEgName, codepage);
		writer.EndObject();
	}
	writer.EndArray();
	writer.Key(L"Constant");
	writer.StartArray();
	for (int i = 0; i < pLibInfo->m_nLibConstCount; i++)
	{
		auto &constant = pLibInfo->m_pLibConst[i];
		writer.StartObject();
		writer.Key(L"Name");
		AnsiString(writer, constant.m_szName, codepage);
		writer.Key(L"EnglshName");
		AnsiString(writer, constant.m_szEgName, codepage);
		switch (constant.m_shtType)
		{
		case CT_NUM:
			writer.Key(L"Value");
			if ((int)constant.m_dbValue == constant.m_dbValue)
				writer.Int((int)constant.m_dbValue);
			else if ((int64_t)constant.m_dbValue == constant.m_dbValue)
				writer.Int64((int64_t)constant.m_dbValue);
			else
				writer.Double(constant.m_dbValue);
			break;
		case CT_BOOL:
			writer.Key(L"Value");
			writer.Bool(constant.m_dbValue != 0);
			break;
		case CT_TEXT:
			writer.Key(L"Value");
			AnsiString(writer, constant.m_szText, codepage);
			break;
		case CT_NULL:
		default:
			writer.Key(L"Value");
			writer.Null();
			break;
		}
		writer.EndObject();
	}
	writer.EndArray();
	writer.EndObject();
	FreeLibrary(fne);

	auto result = s.GetString();
	if (hFile)
	{
		DWORD numberOfBytesWritten;
		WriteFile(hFile, L"\ufeff", sizeof(wchar_t), &numberOfBytesWritten, nullptr);
		WriteFile(hFile, result, s.GetSize(), &numberOfBytesWritten, nullptr);
		CloseHandle(hFile);
	}
	else
	{
		printf("%ws", result);
	}
    return 0;
}

