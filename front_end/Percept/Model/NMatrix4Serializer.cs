using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Percept.Model
{
    // This was used when we were trying to do spatial synchronization - most of that code has been deleted, but some classes remain, because they
    // could be possibly useful in the future.
    public class NMatrix4Serializer : JsonConverter
    {
        private static JsonConverter[] instance = new JsonConverter[] { new NMatrix4Serializer() };

        public static string JsonFromMatrix(OpenTK.NMatrix4 matrix)
        {
            return JsonConvert.SerializeObject(matrix, instance);
        }

        public static OpenTK.NMatrix4 MatrixFromJson(string json)
        {
            return JsonConvert.DeserializeObject<OpenTK.NMatrix4>(json, instance);
        }

        public static string JsonFromMatrix( SceneKit.SCNMatrix4 matrix)
        {
            return JsonConvert.SerializeObject(matrix, instance);
        }

        public static  SceneKit.SCNMatrix4 SCMatrixFromJson(string json)
        {
            return JsonConvert.DeserializeObject< SceneKit.SCNMatrix4>(json, instance);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            OpenTK.NMatrix4? matrix = value as OpenTK.NMatrix4?;
            if (matrix.HasValue)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("M11");
                writer.WriteValue(matrix.Value.M11);
                writer.WritePropertyName("M12");
                writer.WriteValue(matrix.Value.M12);
                writer.WritePropertyName("M13");
                writer.WriteValue(matrix.Value.M13);
                writer.WritePropertyName("M14");
                writer.WriteValue(matrix.Value.M14);

                writer.WritePropertyName("M21");
                writer.WriteValue(matrix.Value.M21);
                writer.WritePropertyName("M22");
                writer.WriteValue(matrix.Value.M22);
                writer.WritePropertyName("M23");
                writer.WriteValue(matrix.Value.M23);
                writer.WritePropertyName("M24");
                writer.WriteValue(matrix.Value.M24);

                writer.WritePropertyName("M31");
                writer.WriteValue(matrix.Value.M31);
                writer.WritePropertyName("M32");
                writer.WriteValue(matrix.Value.M32);
                writer.WritePropertyName("M33");
                writer.WriteValue(matrix.Value.M33);
                writer.WritePropertyName("M34");
                writer.WriteValue(matrix.Value.M34);

                writer.WritePropertyName("M41");
                writer.WriteValue(matrix.Value.M41);
                writer.WritePropertyName("M42");
                writer.WriteValue(matrix.Value.M42);
                writer.WritePropertyName("M43");
                writer.WriteValue(matrix.Value.M43);
                writer.WritePropertyName("M44");
                writer.WriteValue(matrix.Value.M44);

                writer.WriteEndObject();
            }
            else
            {
                SceneKit.SCNMatrix4? scnmatrix = value as SceneKit.SCNMatrix4?;
                if (scnmatrix.HasValue)
                {
                    //copy paste of above
                    writer.WriteStartObject();
                    writer.WritePropertyName("M11");
                    writer.WriteValue(scnmatrix.Value.M11);
                    writer.WritePropertyName("M12");
                    writer.WriteValue(scnmatrix.Value.M12);
                    writer.WritePropertyName("M13");
                    writer.WriteValue(scnmatrix.Value.M13);
                    writer.WritePropertyName("M14");
                    writer.WriteValue(scnmatrix.Value.M14);

                    writer.WritePropertyName("M21");
                    writer.WriteValue(scnmatrix.Value.M21);
                    writer.WritePropertyName("M22");
                    writer.WriteValue(scnmatrix.Value.M22);
                    writer.WritePropertyName("M23");
                    writer.WriteValue(scnmatrix.Value.M23);
                    writer.WritePropertyName("M24");
                    writer.WriteValue(scnmatrix.Value.M24);

                    writer.WritePropertyName("M31");
                    writer.WriteValue(scnmatrix.Value.M31);
                    writer.WritePropertyName("M32");
                    writer.WriteValue(scnmatrix.Value.M32);
                    writer.WritePropertyName("M33");
                    writer.WriteValue(scnmatrix.Value.M33);
                    writer.WritePropertyName("M34");
                    writer.WriteValue(scnmatrix.Value.M34);

                    writer.WritePropertyName("M41");
                    writer.WriteValue(scnmatrix.Value.M41);
                    writer.WritePropertyName("M42");
                    writer.WriteValue(scnmatrix.Value.M42);
                    writer.WritePropertyName("M43");
                    writer.WriteValue(scnmatrix.Value.M43);
                    writer.WritePropertyName("M44");
                    writer.WriteValue(scnmatrix.Value.M44);

                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteNull();
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            if (typeof(OpenTK.NMatrix4).IsAssignableFrom(objectType))
            {
                return new OpenTK.NMatrix4(
                (Single)jObject["M11"], (Single)jObject["M12"], (Single)jObject["M13"], (Single)jObject["M14"],
                (Single)jObject["M21"], (Single)jObject["M22"], (Single)jObject["M23"], (Single)jObject["M24"],
                (Single)jObject["M31"], (Single)jObject["M32"], (Single)jObject["M33"], (Single)jObject["M34"],
                (Single)jObject["M41"], (Single)jObject["M42"], (Single)jObject["M43"], (Single)jObject["M44"]);
            }
            else
            {
                return new SceneKit.SCNMatrix4(
                (Single)jObject["M11"], (Single)jObject["M12"], (Single)jObject["M13"], (Single)jObject["M14"],
                (Single)jObject["M21"], (Single)jObject["M22"], (Single)jObject["M23"], (Single)jObject["M24"],
                (Single)jObject["M31"], (Single)jObject["M32"], (Single)jObject["M33"], (Single)jObject["M34"],
                (Single)jObject["M41"], (Single)jObject["M42"], (Single)jObject["M43"], (Single)jObject["M44"]);
            }
            
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(OpenTK.NMatrix4).IsAssignableFrom(objectType) || typeof(SceneKit.SCNMatrix4).IsAssignableFrom(objectType);
        }

    }
}
