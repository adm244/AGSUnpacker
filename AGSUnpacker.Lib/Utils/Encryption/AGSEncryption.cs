
namespace AGSUnpacker.Lib.Utils.Encryption
{
  public static class AGSEncryption
  {
    private const string hisFriend = "Avis Durgan";
    private const string hisJibzle = "My\x1\xde\x4Jibzle";

    public static string DecryptJibzle(byte[] bufferEncrypted)
    {
      int indexJibzler = 0;
      char[] bufferDecrypted = new char[bufferEncrypted.Length];

      int i;
      for (i = 0; i < bufferEncrypted.Length; ++i)
      {
        byte nextJibzler = (byte)hisJibzle[indexJibzler++];
        byte charDejibzled = (byte)(bufferEncrypted[i] - nextJibzler);
        if (charDejibzled == 0)
          break;

        bufferDecrypted[i] = (char)charDejibzled;
        if (indexJibzler > 10)
          indexJibzler = 0;
      }

      return new string(bufferDecrypted, 0, i);
    }

    //TODO(adm244): write string jibzler

    public static byte[] DecryptAvisBuffer(byte[] bufferEncrypted)
    {
      byte[] bufferDecrypted = new byte[bufferEncrypted.Length];

      for (int i = 0; i < bufferEncrypted.Length; ++i)
      {
        byte salt = (byte)hisFriend[i % hisFriend.Length];
        bufferDecrypted[i] = (byte)(bufferEncrypted[i] - salt);
      }

      return bufferDecrypted;
    }

    public static unsafe string DecryptAvis(byte[] bufferEncrypted)
    {
      byte[] bufferDecrypted = DecryptAvisBuffer(bufferEncrypted);
      return AGSStringUtils.ConvertCString(bufferDecrypted);
    }

    public static byte[] EncryptAvisBuffer(byte[] bufferDecrypted)
    {
      byte[] bufferEncrypted = new byte[bufferDecrypted.Length];

      for (int i = 0; i < bufferDecrypted.Length; ++i)
      {
        byte salt = (byte)hisFriend[i % hisFriend.Length];
        bufferEncrypted[i] = (byte)(bufferDecrypted[i] + salt);
      }

      return bufferEncrypted;
    }

    public static byte[] EncryptAvis(string text)
    {
      byte[] buffer = new byte[text.Length + 1];

      for (int i = 0; i < text.Length; ++i)
        buffer[i] = (byte)text[i];

      //NOTE(adm244): string must be null-terminated before encryption
      buffer[text.Length] = 0;

      return EncryptAvisBuffer(buffer);
    }

    public static string DecryptSalt(string textEncrypted, int salt)
    {
      byte[] bufferDecrypted = new byte[textEncrypted.Length];

      for (int i = 0; i < bufferDecrypted.Length; ++i)
        //NOTE(adm244): convert char to byte before subtracting, so we underflow properly
        bufferDecrypted[i] = (byte)((byte)textEncrypted[i] - salt);

      return AGSStringUtils.ConvertCString(bufferDecrypted);
    }
  }
}
