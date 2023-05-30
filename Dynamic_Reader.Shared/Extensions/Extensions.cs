namespace Dynamic_Reader.Extensions
{
	public static class Extensions
	{
		public static string ToWindowsFilePath(this string text)
		{
			text = text.Replace("/", "\\");
			return text;
		}
	}
}
