﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Uno.Extensions;
using Uno.Logging;

namespace Windows.ApplicationModel.Resources.Core
{
	/// <summary>
	/// Converts a resource candidate to an iOS resource path.
	/// </summary>
	internal class iOSResourceConverter
	{
		public static string Convert(ResourceCandidate resourceCandidate, string defaultLanguage)
		{
			try
			{
				if (IsImageAsset(resourceCandidate.LogicalPath))
				{
					ValidatePlatform(resourceCandidate);

					var language = GetLanguage(resourceCandidate);
					var directory = Path.GetDirectoryName(resourceCandidate.LogicalPath);
					var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resourceCandidate.LogicalPath);
					var scale = GetScale(resourceCandidate);
					var extension = Path.GetExtension(resourceCandidate.LogicalPath).ToLowerInvariant();

					return Path.Combine(language, directory, $"{fileNameWithoutExtension}{scale}{extension}");
				} else if (IsFontAsset(resourceCandidate.LogicalPath))
				{
					var directory = Path.GetDirectoryName(resourceCandidate.LogicalPath);
					string path = Path.Combine(directory, Path.GetFileName(resourceCandidate.LogicalPath));
					return path;
				} else
				{
					throw new Exception("Unsupported asset type");
				}
			}
			catch (Exception ex)
			{
				ex.Log().Info($"Couldn't convert {resourceCandidate.ValueAsString} to an iOS resource path.", ex);
				return null;
			}
		}

		private static void ValidatePlatform(ResourceCandidate resourceCandidate)
		{
			var custom = resourceCandidate.GetQualifierValue("custom");

			if (custom != null && custom != "ios")
			{
				throw new NotSupportedException($"Custom qualifier of value {custom} is not supported on iOS.");
			}
		}

		private static string GetLanguage(ResourceCandidate resourceCandidate)
		{
			var language = resourceCandidate.GetQualifierValue("language");

			if (language == null)
			{
				return "";
			}

			if (language.Contains("-"))
			{
				language = language.Replace("-", "_");
			}

			return $"{language}.lproj";
		}

		private static string GetScale(ResourceCandidate resourceCandidate)
		{
			var scale = resourceCandidate.GetQualifierValue("scale");

			switch (scale)
			{
				case null:
				case "100":
					return "";
				case "200":
					return "@2x";
				case "300":
					return "@3x";
				default:
					throw new NotSupportedException($"Scale qualifier of value {scale} is not supported on iOS.");
			}
		}

		private static bool IsImageAsset(string path)
		{
			var extension = Path.GetExtension(path).ToLowerInvariant();
			return extension == ".png"
				|| extension == ".jpg"
				|| extension == ".jpeg"
				|| extension == ".gif";
		}

		private static bool IsFontAsset(string path)
		{
			var extension = Path.GetExtension(path).ToLowerInvariant();
			return extension == ".ttf"
				|| extension == ".eot"
				|| extension == ".woff"
				|| extension == ".woff2";
		}
	}
}
