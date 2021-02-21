namespace Core.Common.Misc {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using UnityEngine;

	//TODO: разбить на partial классы
	public static class Utils {
		
		//--------------------------------------------------------------------------------------------------------------
		#region UI Utils
		
		public static class UI {
			public static Vector2 UIScreenSize {
				get {
					float width = 0;
					float height = 0;

					bool potrait = Screen.height > Screen.width;

					float u = potrait
						? (float)Screen.height / Screen.width
						: (float)Screen.width / Screen.height;
                
					if (u >= 1.5f) {
						width = 750;
						height = u * width;
					} else {
						height = 1334;
						width = height / u;
					}
					return  potrait
						? new Vector2(Mathf.RoundToInt(width), Mathf.RoundToInt(height))
						: new Vector2(Mathf.RoundToInt(height), Mathf.RoundToInt(width));
				}
			}
		
			public static Vector2 InputToVirtualCoords(Vector2 input) {
				return InputToVirtualCoords(input.x, input.y);
			}

			public static Vector2 InputToVirtualCoords(float x, float y) {
				var uiSS = UIScreenSize;
				return new Vector2(
					uiSS.x * x / Screen.width,
					uiSS.y * y / Screen.height
				);
			}
			
			public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to) {
				Vector2 localPoint;
            
				Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin,
					from.rect.height * from.pivot.y + from.rect.yMin);
            
				Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
				screenP += fromPivotDerivedOffset;
            
				RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
            
				Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin,
					to.rect.height * to.pivot.y + to.rect.yMin);
            
				return to.anchoredPosition + localPoint - pivotDerivedOffset;
			}
		}
		
		#endregion
		//--------------------------------------------------------------------------------------------------------------
		
		//--------------------------------------------------------------------------------------------------------------
		#region Time

		public static class Time {
			public static double SecondsNow() {
				return DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
			}

			public static DateTime UnixTimestampToDateTime(string timestamp) {             
				DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				dtDateTime = dtDateTime.AddMilliseconds(Convert.ToDouble(timestamp));
				return dtDateTime;
			}

			public static double ConvertDateTimeToUnixTimestamp(DateTime dateTime) {             
				DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				TimeSpan diff = dateTime.ToUniversalTime() - origin;
				return System.Math.Floor(diff.TotalMilliseconds);
			}
		}

		#endregion
		//--------------------------------------------------------------------------------------------------------------


		//--------------------------------------------------------------------------------------------------------------
		#region Math

		public static class Math {
			public static bool IsPlaneDistCloser(Vector3 a, Vector3 b, float dist) {
				float xSide = b.x - a.x;
				float zSide = a.z - b.z;

				return xSide * xSide + zSide * zSide < dist * dist;
			}

			public static bool IsPlaneDistFarther(Vector3 a, Vector3 b, float dist) {
				return false == IsPlaneDistCloser(a, b, dist);
			}
			
			public static float PlaneDistSquared(Vector3 a, Vector3 b) {
				float xSide = b.x - a.x;
				float zSide = a.z - b.z;

				return xSide * xSide + zSide * zSide;
			}
			
			public static float PlaneDistSquared2D(Vector3 a, Vector3 b) {
				float xSide = b.x - a.x;
				float ySide = a.y - b.y;

				return xSide * xSide + ySide * ySide;
			}
			
			public static Vector2 GetProjection(Vector2 point, Vector2 a, Vector2 b) {
				float fDenominator = (b.x - a.x)*(b.x - a.x) + (b.y - a.y)*(b.y - a.y);
				if (fDenominator == 0) // p1 and p2 are the same
					return a;

				float t = (point.x*(b.x - a.x) - (b.x - a.x)*a.x + point.y*(b.y - a.y) - (b.y - a.y)*a.y) / fDenominator;

				return new Vector2(a.x + (b.x - a.x)*t, a.y + (b.y - a.y)*t);
			}
		}

		#endregion
		//--------------------------------------------------------------------------------------------------------------

		//--------------------------------------------------------------------------------------------------------------
		#region Physics
		public static LayerMask GetPhysicsLayerMask(int currentLayer)
		{
			int finalMask = 0;
			for (int i = 0; i < 32; i++)
			{
				if (!Physics.GetIgnoreLayerCollision(currentLayer, i)) finalMask = finalMask | (1 << i);
			}
			return finalMask;
		}
		#endregion
		//--------------------------------------------------------------------------------------------------------------

		//--------------------------------------------------------------------------------------------------------------
		#region String

		public static class String {
			public static bool DoesContainCyrillicSymbols(string text) {
				return Regex.IsMatch(text, "[а-яА-ЯеЁ]");
			}
			
			public static string RandomString(int length) {
				var random = new System.Random();
				const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
				return new string(Enumerable.Repeat(chars, length)
					.Select(s => s[random.Next(s.Length)]).ToArray());
			}
		}
		
		/// <summary>
	    /// Конвертация float в string с сокращениями (1000 = 1K, 1000K = 1M)
	    /// </summary>
	    /// <returns>Форматированная строка</returns>
	    /// <param name="value">Число для преобразования</param>
	    public static string FormatFloat(float value) {
	        string result = string.Empty;

	        if (value < 100f) {
	            result = value.ToString("0.#");
	        }
	        else if (value < 1000f) {
	            result = value.ToString("0");
	        }
	        else if (value < 10000f) {
	            result = (value / 1000f).ToString("0") + "k";
	        }
	        else if (value < 1000000f) {
	            result = (value / 1000f).ToString("0") + "k";
	        }
	        else if (value < 10000000f) {
	            result = (value / 1000000f).ToString("0") + "m";
	        }
	        else if (value >= 10000000f) {
	            result = (value / 1000000f).ToString("0") + "m";
	        }
	        else if (value >= 1E+9f) {
	            result = (value / 1E+9f).ToString("0") + "b";
	        }
	        else if (value >= 1E+12f) {
	            result = (value / 1E+12f).ToString("0") + "tr";
	        }
	        else if (value >= 1E+15f) {
	            result = (value / 1E+15f).ToString("0") + "qa";
	        }
	        else if (value >= 1E+18f) {
	            result = (value / 1E+18f).ToString("0") + "qu";
	        }
	        else if (value >= 1E+21f) {
	            result = (value / 1E+21f).ToString("0") + "sp";
	        }
	        else if (value >= 1E+24f) {
	            result = (value / 1E+24f).ToString("0") + "sp";
	        }
	        else if (value >= 1E+27f) {
	            result = (value / 1E+27f).ToString("0") + "ot";
	        }
	        else if (value >= 1E+30f) {
	            result = (value / 1E+30f).ToString("0") + "nn";
	        }
	        else if (value >= 1E+33f) {
	            result = (value / 1E+33f).ToString("0") + "dc";
	        }
			else if (value >= 1E+36f) {
	            result = (value / 1E+36f).ToString("0") + "ud";
	        }

	        return result;
	    }
		
		/// <summary>
		/// Конфертация string в float с учётом разделителя и в виде точки, и в виде запятой
		/// </summary>
		/// <returns>float</returns>
		/// <param name="value">Строка для преобразования</param>
		public static float StringToFloat(string value) {
			value = value.Replace(',', '.');
			return float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		#endregion
		//--------------------------------------------------------------------------------------------------------------

		
		//--------------------------------------------------------------------------------------------------------------
		#region App

		public static class App {
			public static Tuple<int, int, int> ParseVersion(string version) {
				version = Regex.Replace(version, "[A-Za-z ]", "");
				var versionSplit = version.Split('.');

				var v1 = int.Parse(versionSplit[0]);
				var v2 = int.Parse(versionSplit[1]);

				Tuple<int, int, int> outer;
				
				#if DEVELOPMENT_BUILD
				if (versionSplit.Length > 2) {
					var v3 = int.Parse(versionSplit[2]);
					outer = new Tuple<int, int, int>(v1, v2, v3);
				}
				else
					outer = new Tuple<int, int, int>(v1, v2, 0);
				#else
				outer = new Tuple<int, int, int>(v1, v2, 0);
				#endif

				return outer;
			}

			public static bool IsVersionPlayable(Tuple<int, int, int> neededVersion, Tuple<int, int, int> appVersion) {
				bool isPlayable;

				if (neededVersion.Item1 > appVersion.Item1) {
					isPlayable = false;
				}
				else {
					if (neededVersion.Item2 > appVersion.Item2) {
						isPlayable = false;
					}
					else {
						if (neededVersion.Item3 > appVersion.Item3) {
							isPlayable = false;
						}
						else {
							isPlayable = true;
						}
					}
				}

				return isPlayable;
			}
			
			/// <summary>
			/// Consent data for IronSourceController (Ads)
			/// </summary>
			public static bool GdprConsent {
				get {
					if (PlayerPrefs.HasKey("GDPRConsent"))
						return PlayerPrefs.GetInt("GDPRConsent") == 1;
					return false;
				}
				set => PlayerPrefs.SetInt("GDPRConsent", value ? 1 : 0);
			}
		}

		#endregion
		//--------------------------------------------------------------------------------------------------------------

		//--------------------------------------------------------------------------------------------------------------
		#region Material

		public enum BlendMode
		{
			Opaque,
			Cutout,
			Fade,
			Transparent
		}

		public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
		{
			switch (blendMode)
			{
				case BlendMode.Opaque:
					standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					standardShaderMaterial.SetInt("_ZWrite", 1);
					standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
					standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
					standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					standardShaderMaterial.renderQueue = -1;
					break;
				case BlendMode.Cutout:
					standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					standardShaderMaterial.SetInt("_ZWrite", 1);
					standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
					standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
					standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					standardShaderMaterial.renderQueue = 2450;
					break;
				case BlendMode.Fade:
					standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					standardShaderMaterial.SetInt("_ZWrite", 0);
					standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
					standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
					standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					standardShaderMaterial.renderQueue = 3000;
					break;
				case BlendMode.Transparent:
					standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					standardShaderMaterial.SetInt("_ZWrite", 0);
					standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
					standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
					standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
					standardShaderMaterial.renderQueue = 3000;
					break;
			}

		}
		#endregion
		//--------------------------------------------------------------------------------------------------------------
	}
}



