using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace PathsOfDelight
{
    public sealed class LocalVault
    {
        private const string KeyPreference = "pod_local_key_v1";
        private readonly string _path = Path.Combine(Application.persistentDataPath, "paths.save");

        public LocalSave Load()
        {
            try
            {
                if (!File.Exists(_path)) return NewSave();
                var raw = Convert.FromBase64String(File.ReadAllText(_path));
                if (raw.Length < 49) return NewSave();
                var key = GetOrCreateKey();
                var iv = new byte[16];
                var mac = new byte[32];
                var cipher = new byte[raw.Length - 48];
                Buffer.BlockCopy(raw, 0, iv, 0, 16);
                Buffer.BlockCopy(raw, 16, mac, 0, 32);
                Buffer.BlockCopy(raw, 48, cipher, 0, cipher.Length);
                var expected = ComputeMac(key, iv, cipher);
                if (!ConstantTimeEquals(mac, expected)) return NewSave();
                var save = JsonUtility.FromJson<LocalSave>(Decrypt(key, iv, cipher));
                return save ?? NewSave();
            }
            catch
            {
                return NewSave();
            }
        }

        public void Save(LocalSave save)
        {
            var key = GetOrCreateKey();
            var iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(iv);
            var cipher = Encrypt(key, iv, JsonUtility.ToJson(save));
            var mac = ComputeMac(key, iv, cipher);
            var raw = new byte[iv.Length + mac.Length + cipher.Length];
            Buffer.BlockCopy(iv, 0, raw, 0, iv.Length);
            Buffer.BlockCopy(mac, 0, raw, iv.Length, mac.Length);
            Buffer.BlockCopy(cipher, 0, raw, iv.Length + mac.Length, cipher.Length);
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(_path, Convert.ToBase64String(raw));
        }

        public void DeleteAll()
        {
            if (File.Exists(_path)) File.Delete(_path);
            PlayerPrefs.DeleteKey(KeyPreference);
            PlayerPrefs.Save();
        }

        private static LocalSave NewSave()
        {
            var save = new LocalSave();
            save.session.sessionId = Guid.NewGuid().ToString("N");
            save.session.seed = Environment.TickCount & 0x7fffffff;
            return save;
        }

        private static byte[] GetOrCreateKey()
        {
            var existing = PlayerPrefs.GetString(KeyPreference, string.Empty);
            if (!string.IsNullOrEmpty(existing)) return Convert.FromBase64String(existing);
            var key = new byte[64];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(key);
            PlayerPrefs.SetString(KeyPreference, Convert.ToBase64String(key));
            PlayerPrefs.Save();
            return key;
        }

        private static byte[] Encrypt(byte[] key, byte[] iv, string plaintext)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Slice(key, 0, 32);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var encryptor = aes.CreateEncryptor())
                {
                    var input = Encoding.UTF8.GetBytes(plaintext);
                    return encryptor.TransformFinalBlock(input, 0, input.Length);
                }
            }
        }

        private static string Decrypt(byte[] key, byte[] iv, byte[] cipher)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Slice(key, 0, 32);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var decryptor = aes.CreateDecryptor())
                {
                    var output = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                    return Encoding.UTF8.GetString(output);
                }
            }
        }

        private static byte[] ComputeMac(byte[] key, byte[] iv, byte[] cipher)
        {
            using (var hmac = new HMACSHA256(Slice(key, 32, 32)))
            {
                var data = new byte[iv.Length + cipher.Length];
                Buffer.BlockCopy(iv, 0, data, 0, iv.Length);
                Buffer.BlockCopy(cipher, 0, data, iv.Length, cipher.Length);
                return hmac.ComputeHash(data);
            }
        }

        private static byte[] Slice(byte[] source, int offset, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(source, offset, result, 0, length);
            return result;
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
