// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

namespace Microsoft.Graph.ODataTemplateWriter.CodeHelpers.ObjC
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Graph.ODataTemplateWriter.Extensions;
    using Microsoft.Graph.ODataTemplateWriter.Settings;
    using Vipr.Core.CodeModel;
    using NLog;

    public static class TypeHelperObjC
    {
        public static string Prefix = ConfigurationService.Settings.NamespacePrefix;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static ICollection<string> reservedNames;
        public static ICollection<string> ReservedNames
        {
            get
            {
                if (reservedNames == null)
                {
                    reservedNames=new HashSet<string>
                    {
                        "id",
                        "YES",
                        "NO",
                        "true",
                        "false",
                        "NULL",
                        "nil",
                        "self",
                        "description",
                        "auto",
                        "else",
                        "long",
                        "switch",
                        "break",
                        "enum",
                        "register",
                        "typedef",
                        "case",
                        "extern",
                        "return",
                        "union",
                        "char",
                        "float",
                        "short",
                        "unsigned",
                        "const",
                        "for",
                        "signed",
                        "void",
                        "continue",
                        "goto",
                        "sizeof",
                        "volatile",
                        "default",
                        "if",
                        "static",
                        "while",
                        "do",
                        "int",
                        "struct",
                        "_Packed",
                        "double",
                        "protocol",
                        "interface",
                        "implementation",
                        "NSObject",
                        "NSInteger",
                        "NSNumber",
                        "CGFloat",
                        "property",
                        "nonatomic",
                        "retain",
                        "weak",
                        "unsafe_unretained",
                        "readwrite",
                        "readonly",
                        "inline",
                        "operations",
                        "Duration",
                        "False"

                    };
                }
                return reservedNames;
            }
        }

        public static string GetTypeString(this OdcmType type)
        {
            if (type == null)
            {
                return "id";
            }
            switch (type.Name) {
                case "String":
                    return "NSString";
                case "Int32":
                    return "int32_t";
                case "Int64":
                    return "int64_t";
                case "Int16":
                    return "int16_t";
                case "Guid":
                    return "NSString";
                case "Double":
                case "Float":
                case "Single":
                    return "double";
                case "DateTimeOffset":
                    return "NSDate";
                case "Date":
                    return "MSDate";
                case "TimeOfDay":
                    return "MSTimeOfDay";
                case "Binary":
                    return "NSString";
                case "Boolean":
                    return "BOOL";
                case "Stream":
                    return "NSStream";
                case "Duration":
                    return "MSDuration";
                case "NSDictionary":
                    return "NSDictionary";
                case "JSON":
                    return "NSDictionary";
                case "Json":
                    return "NSDictionary";
                case "Byte":
                    return "Byte";

                default:
                    return Prefix + type.Name.ToUpperFirstChar();
            }
        }

        public static string GetTypeString(this OdcmProperty property)
        {
            return property.Projection.Type.GetTypeString();
        }

        public static bool IsComplex(this OdcmType type)
        {
            string t = GetTypeString(type);
            return
                !(t == "int32_t" || t == "int64_t" || t == "int16_t" || t == "BOOL" || t == "Byte" || t == "double");
        }

        public static bool IsComplex(this OdcmProperty property)
        {
            return property.Projection.Type.IsComplex();
        }

        public static bool IsPrimitive(this OdcmType type)
        {
            return !type.IsComplex();
        }

        public static string ToSetterTypeString(this OdcmProperty property)
        {
            return string.Format("{0} {1}", property.GetFullType(), (property.IsComplex() ? "*" : string.Empty));
        }

        public static string SanitizePropertyName(this OdcmProperty property)
        {
            if (ReservedNames.Contains(property.Name.ToLower()))
            {
                logger.Info("Property \"{0}\" is a reserved word in Objective-C. Converting to \"{1}{0}\"", 
                    property.Name.ToUpperFirstChar(), 
                    property.Class.Name.ToLowerFirstChar()
                );
                return property.Class.Name.ToLowerFirstChar() + property.Name.ToUpperFirstChar();
            }
            return property.Name;
        }

        public static string GetFullType(this OdcmProperty property)
        {
            if (property.IsCollection)
            {
                return  "NSMutableArray";
            }
            else
            {
                return property.Projection.Type.GetTypeString();
            }
        }

        public static string GetFullType(this OdcmType type)
        {
            return GetTypeString(type);
        }

        public static bool IsSystem(this OdcmProperty property)
        {
            return property.Projection.Type.IsSystem();
        }

        public static bool IsSystem(this OdcmType type)
        {
            string t = GetTypeString(type);
            return (t == "int32_t" || t == "int64_t" || t == "int16_t" || t == "BOOL" || t == "Byte" || t == "NSString" || t == "NSDate" || t == "NSStream" || t == "double");
        }

        public static bool IsComplexCollectionOpenType(this OdcmProperty property, OdcmModel model)
        {
            return property.IsComplex() && property.Projection.Type.Name.ToLower().EndsWith("collection") &&
                model.GetComplexTypes().Any(complexType => complexType.Name.Equals(property.Projection.Type.Name) && complexType.IsOpen);
        }

        public static bool IsDate(this OdcmProperty prop)
        {
            return prop.Projection.Type.IsDate();
        }

        public static bool IsDate(this OdcmType type)
        {
            string typeString = GetTypeString(type);
            return typeString.Equals("NSDate");
        }

        public static string GetToLowerFirstCharName(this OdcmProperty property)
        {
            return property.Name.ToLowerFirstChar();
        }

        public static string GetToLowerImport(this OdcmProperty property)
        {
            var type = property.Projection.Type;
            var index = type.Name.LastIndexOf('.');
            return type.Name.Substring(0, index).ToLower() + type.Name.Substring(index);
        }

        public static string GetToUpperFirstCharName(this OdcmProperty property)
        {
            return property.Name.ToUpperFirstChar();
        }

        public static bool IsEnum(this OdcmProperty property)
        {
            return property.Projection.Type is OdcmEnum;
        }
        public static string GetNSNumberValueMethod(this OdcmType type)
        {
            string objectiveCType = type.GetTypeString();
            if (objectiveCType.Equals("int32_t") || objectiveCType.Equals("int16_t"))
            {
                return "intValue";
            }
            if (objectiveCType.Equals("int64_t"))
            {
                return "longLongValue";
            }
            else if(objectiveCType.Equals("BOOL"))
            {
                return "boolValue";
            }
            else if (objectiveCType.Equals("double"))
            {
                return "floatValue";
            }
            else if (type is OdcmEnum)
            {
                return "intValue";
            }

            return null;
        }
    }
}
