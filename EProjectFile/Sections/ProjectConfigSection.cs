﻿using System.Text.Json;
using QIQI.EProjectFile.Internal;
using System;
using System.IO;
using System.Text;
using QIQI.EProjectFile.Context;

namespace QIQI.EProjectFile.Sections
{
    public class ProjectConfigSection : ISection
    {
        private class KeyImpl : ISectionKey<ProjectConfigSection>
        {
            public string SectionName => "用户信息段";
            public int SectionKey => 0x01007319;
            public bool IsOptional => false;

            public ProjectConfigSection Parse(BlockParserContext context)
            {
                return context.Consume(reader =>
                {
                    var encoding = context.Encoding;
                    var projectConfig = new ProjectConfigSection();
                    projectConfig.Name = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Description = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Author = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.ZipCode = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Address = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.TelephoneNumber = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.FaxNumber = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Email = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Homepage = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Copyright = reader.ReadStringWithLengthPrefix(encoding);
                    projectConfig.Version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    projectConfig.WriteVersion = reader.ReadInt32() == 0;
                    projectConfig.CompilePlugins = reader.ReadStringWithFixedLength(encoding, 20);
                    projectConfig.ExportPublicClassMethod = reader.ReadInt32() != 0;
                    reader.ReadInt32(); // Unknown
                    return projectConfig;
                });
            }
        }

        public static readonly ISectionKey<ProjectConfigSection> Key = new KeyImpl();
        public string SectionName => Key.SectionName;
        public int SectionKey => Key.SectionKey;
        public bool IsOptional => Key.IsOptional;

        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string TelephoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Email { get; set; }
        public string Homepage { get; set; }
        public string Copyright { get; set; }
        public Version Version { get; set; }
        public bool WriteVersion { get; set; }
        public string CompilePlugins { get; set; }
        public bool ExportPublicClassMethod { get; set; }
        public byte[] ToBytes(BlockByteifierContext context)
        {
            return context.Collect(writer => 
            {
                var encoding = context.Encoding;
                writer.WriteStringWithLengthPrefix(encoding, Name);
                writer.WriteStringWithLengthPrefix(encoding, Description);
                writer.WriteStringWithLengthPrefix(encoding, Author);
                writer.WriteStringWithLengthPrefix(encoding, ZipCode);
                writer.WriteStringWithLengthPrefix(encoding, Address);
                writer.WriteStringWithLengthPrefix(encoding, TelephoneNumber);
                writer.WriteStringWithLengthPrefix(encoding, FaxNumber);
                writer.WriteStringWithLengthPrefix(encoding, Email);
                writer.WriteStringWithLengthPrefix(encoding, Homepage);
                writer.WriteStringWithLengthPrefix(encoding, Copyright);
                writer.Write(Version.Major);
                writer.Write(Version.Minor);
                writer.Write(Version.Build);
                writer.Write(Version.Revision);
                writer.Write(WriteVersion ? 0 : 1);
                writer.WriteStringWithFixedLength(encoding, CompilePlugins, 20);
                writer.Write(ExportPublicClassMethod ? 1 : 0);
                writer.Write(0); // Unknown
            });
        }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonUtils.Options);
        }
    }
}