#ifndef __CLI_H__
#define __CLI_H__

#include "sol.h"

namespace CefFlashBrowser::Sol
{
    using namespace System;
    using namespace System::Collections::Generic;


    public enum class SolVersion
    {
        AMF0 = (int)sol::SolVersion::AMF0,
        AMF3 = (int)sol::SolVersion::AMF3
    };


    public ref class SolUndefined sealed
    {
    private:
        static SolUndefined^ _value = gcnew SolUndefined();
        SolUndefined() {}

    public:
        static property SolUndefined^ Value { SolUndefined^ get() { return _value; } }
        virtual bool Equals(Object^ obj) override { return obj != nullptr && obj->GetType() == SolUndefined::typeid; }
        virtual int GetHashCode() override { return 0; }
        virtual String^ ToString() override { return "undefined"; }
        static bool operator ==(SolUndefined^ left, SolUndefined^ right) { return true; }
        static bool operator !=(SolUndefined^ left, SolUndefined^ right) { return false; }
    };


    public ref class SolXmlDoc sealed
    {
    private:
        String^ _xml;

    public:
        SolXmlDoc(String^ xml) : _xml(xml) {}
        ~SolXmlDoc() {}

    public:
        property String^ Data {
            String^ get() { return _xml; }
            void set(String^ value) { _xml = value; }
        }
        virtual String^ ToString() override { return _xml; }
    };


    public ref class SolXml sealed
    {
    private:
        String^ _xml;

    public:
        SolXml(String^ xml) : _xml(xml) {}
        ~SolXml() {}

    public:
        property String^ Data {
            String^ get() { return _xml; }
            void set(String^ value) { _xml = value; }
        }
        virtual String^ ToString() override { return _xml; }
    };


    public ref class SolValueWrapper sealed
    {
    internal:
        sol::SolValue* _pval;
        SolValueWrapper(sol::SolValue* pval);

    public:
        SolValueWrapper();
        ~SolValueWrapper();

    public:
        property Type^ Type { System::Type^ get(); }
        property bool IsUndefined { bool get(); }
        property bool IsNull { bool get(); }

        Object^ GetValue();
        SolValueWrapper^ SetValue(Object^ value);
    };


    public ref class SolFileWrapper sealed
    {
    private:
        Dictionary<String^, SolValueWrapper^>^ _data;

    internal:
        sol::SolFile* _pfile;
        SolFileWrapper(sol::SolFile* pfile);

        void UpdateUnmanagedData();

    public:
        SolFileWrapper(String^ path);
        ~SolFileWrapper();

    public:
        property String^ Path { String^ get(); void set(String^ value); }
        property String^ SolName { String^ get(); void set(String^ value); }
        property SolVersion Version { SolVersion get(); void set(SolVersion value); }
        property Dictionary<String^, SolValueWrapper^>^ Data { Dictionary<String^, SolValueWrapper^>^ get(); }

        void Save();
        static SolFileWrapper^ ReadFile(String^ path);
        static SolFileWrapper^ CreateEmpty(String^ path);
    };


    public ref class SolArrayWrapper sealed
    {
    private:
        List<SolValueWrapper^>^ _dense;
        Dictionary<String^, SolValueWrapper^>^ _assoc;

    internal:
        sol::SolArray* _parr;
        SolArrayWrapper(sol::SolArray* parr);

        void UpdateUnmanagedData();

    public:
        SolArrayWrapper();
        ~SolArrayWrapper();

    public:
        property List<SolValueWrapper^>^ Dense { List<SolValueWrapper^>^ get(); }
        property Dictionary<String^, SolValueWrapper^>^ Assoc { Dictionary<String^, SolValueWrapper^>^ get(); }
    };


    public ref class SolObjectWrapper sealed
    {
    private:
        String^ _class;
        Dictionary<String^, SolValueWrapper^>^ _props;

    internal:
        sol::SolObject* _pobj;
        SolObjectWrapper(sol::SolObject* pobj);

        void UpdateUnmanagedData();

    public:
        SolObjectWrapper();
        ~SolObjectWrapper();

    public:
        property String^ Class { String^ get(); void set(String^ value); }
        property Dictionary<String^, SolValueWrapper^>^ Props { Dictionary<String^, SolValueWrapper^>^ get(); }
    };
}

#endif // !__CLI_H__
