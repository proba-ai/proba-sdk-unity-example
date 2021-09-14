using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ProbaSDK.Internal
{
    internal enum JwtHashAlgorithm
    {
        HS256,
        HS384,
        HS512
    }

    internal class JsonWebToken
    {
        private static Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;

        static JsonWebToken()
        {
            HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>
            {
                { JwtHashAlgorithm.HS256, (key, value) => { using (var sha = new HMACSHA256(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS384, (key, value) => { using (var sha = new HMACSHA384(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS512, (key, value) => { using (var sha = new HMACSHA512(key)) { return sha.ComputeHash(value); } } }
            };
        }

        public static string Encode(string payload, string key, JwtHashAlgorithm algorithm)
        {
            return Encode(Encoding.UTF8.GetBytes(payload), Encoding.UTF8.GetBytes(key), algorithm);
        }

        public static string Encode(byte[] payloadBytes, byte[] keyBytes, JwtHashAlgorithm algorithm)
        {
            var segments = new List<string>();
            var header = $"{{\"alg\": \"{algorithm}\",\"typ\":\"JWT\"}}";
            
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));

            var stringToSign = string.Join(".", segments.ToArray());

            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            byte[] signature = HashAlgorithms[algorithm](keyBytes, bytesToSign);
            segments.Add(Base64UrlEncode(signature));

            return string.Join(".", segments.ToArray());
        }

        // from JWT spec
        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }
    }
}