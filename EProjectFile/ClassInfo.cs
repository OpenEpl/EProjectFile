using Newtonsoft.Json;
using System;
using System.IO;

namespace QIQI.EProjectFile
{
    public class ClassInfo : IHasId
    {
        private int id;
        public int Id => id;
        public ClassInfo(int id)
        {
            this.id = id;
        }

        [JsonIgnore]
        public int UnknownAfterId;
        [JsonIgnore]
        public int UnknownBeforeBaseClass;
        public int BaseClass;
        public string Name;
        public string Comment;
        public int[] Method;
        public VariableInfo[] Variables;

        public static ClassInfo[] ReadClasses(BinaryReader reader)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var classes = new ClassInfo[count];
            for (int i = 0; i < count; i++)
            {
                var classInfo = new ClassInfo(ids[i])
                {
                    UnknownAfterId = unknownsAfterIds[i],
                    UnknownBeforeBaseClass = reader.ReadInt32(),
                    BaseClass = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(),
                    Comment = reader.ReadStringWithLengthPrefix(),
                    Method = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4),
                    Variables = VariableInfo.ReadVariables(reader)
                };
                classes[i] = classInfo;
            }

            return classes;
        }
        public static void WriteClasses(BinaryWriter writer, ClassInfo[] classes)
        {
            writer.Write(classes.Length * 8);
            Array.ForEach(classes, x => writer.Write(x.Id));
            Array.ForEach(classes, x => writer.Write(x.UnknownAfterId));
            foreach (var classInfo in classes)
            {
                writer.Write(classInfo.UnknownBeforeBaseClass);
                writer.Write(classInfo.BaseClass);
                writer.WriteStringWithLengthPrefix(classInfo.Name);
                writer.WriteStringWithLengthPrefix(classInfo.Comment);
                if (classInfo.Method == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(classInfo.Method.Length * 4);
                    writer.WriteInt32sWithoutLengthPrefix(classInfo.Method);
                }
                VariableInfo.WriteVariables(writer, classInfo.Variables);
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
