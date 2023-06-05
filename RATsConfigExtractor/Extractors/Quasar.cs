using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RATsConfigExtractor.Extractors
{
    internal class Quasar
    {
        private static ModuleDefMD Module = null;
        private static Assembly ASM = null;

        private static TypeDef configType = null;
        private static MethodDef importantMethod = null;

        static Dictionary<String, object> Fields = new Dictionary<String, object>();

        public static void Execute(string filePath)
        {
            Module = ModuleDefMD.Load(filePath);
            ASM = Assembly.LoadFrom(filePath);

            foreach (TypeDef Type in Module.Types.Where(T => !T.IsGlobalModuleType && T.HasMethods && T.Namespace == string.Empty))
            {
                if (Type.Fields.Count == 24 && Type.Methods.Count == 4)
                    configType = Type;
                if (Type.Methods.Count == 6 && Type.Fields.Count == 7)
                    importantMethod = Type.Methods.Where(M => M.HasBody && M.Body.HasInstructions && M.Body.Instructions.Count() < 10 && M.Body.Instructions[0].ToString().Contains("System.Text.Encoding::get_UTF8")).First();
            }

            Console.WriteLine($"Found config type ! MDToken: {configType.MDToken}");
            Console.WriteLine($"Found important method ! MDToken: {importantMethod.MDToken}");
            
            MethodDef configTypeConstructor = configType.FindOrCreateStaticConstructor();
            string encryptionKey = string.Empty;

            string fieldName = string.Empty;

            configTypeConstructor.Body.Instructions.ToList().ForEach(instruction =>
            {
                if (instruction.OpCode.OperandType == OperandType.InlineString)
                    Fields.Add(fieldName, instruction.Operand.ToString());
                else if (instruction.OpCode.OperandType == OperandType.InlineField)
                    fieldName = (instruction.Operand as MemberRef)?.Name;
            });

            foreach (var prop in Fields)
                if (prop.Value.ToString().Where(c => !Char.IsDigit(c)).All(c => Char.IsUpper(c)) && prop.Value.ToString() != string.Empty && encryptionKey == string.Empty)
                    encryptionKey = prop.Value.ToString();

            Console.WriteLine($"Found Encryption Key ! Key: {encryptionKey}");

            foreach (KeyValuePair<String, object> KP in Fields)
                try
                {
                    if ((KP.Value is String) && (KP.Key != fieldName))
                    {
                        MethodInfo method = (MethodInfo)ASM.ManifestModule.ResolveMethod((int)importantMethod.MDToken.Raw);

                        if (method.IsStatic)
                            Console.WriteLine((string)method.Invoke(null, new object[] { KP.Value }));
                        else
                        {
                            if (method.DeclaringType.GetConstructor(new Type[] { typeof(string) }) is var constructor != null)
                            {
                                var targetObject = constructor.Invoke(new object[] { encryptionKey });

                                var decrypted = (string)method.Invoke(targetObject, new object[] { KP.Value });
                                if (decrypted.Length < 37)
                                    Console.WriteLine(decrypted);
                            }
                        }
                    }
                }
                catch (Exception ex) { }
        }
    }
}