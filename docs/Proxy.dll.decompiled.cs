#define TRACE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Eto.Drawing;
using Eto.Forms;
using Jose;
using McNeel.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.Runtime.RhinoAccounts;
using Rhino.UI;
using Rhino.UI.Runtime;
using WebSocketSharp;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyFileVersion("8.30.26103.11001")]
[assembly: AssemblyInformationalVersion("Rhino 8.30")]
[assembly: AssemblyTitle("Proxy")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Robert McNeel & Associates")]
[assembly: AssemblyProduct("Proxy")]
[assembly: AssemblyCopyright("Copyright © 1993-2020, Robert McNeel & Associates")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("4c1c57d5-63af-4254-b664-791a483bf518")]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("8.30.26103.11001")]
[module: UnverifiableCode]
internal static class RhinoBuildConstants
{
	public const string VERSION_STRING = "8.30.26103.11001";

	public const string MAJOR_MINOR_VERSION_STRING = "8.30";

	public const string MAJOR_VERSION_STRING = "8";

	public const int MAJOR_VERSION = 8;

	public const int MINOR_VERSION = 30;

	public const int YEAR = 2026;

	public const int MONTH = 4;

	public const int DAY_OF_MONTH = 13;

	public const int HOUR = 11;

	public const int MINUTE = 0;

	public const bool PRE_RELEASE = false;

	public const bool USE_COMMERCIAL_LICENSE = true;

	public const string GoogleAnalyticsRhinoTrackingID = "UA-512742-12";

	public static readonly Guid LICENSE_GUID_V7_COMMERCIAL = new Guid("59FF75C9-9C71-4EF8-A290-6B590F3FC63A");

	public static readonly Guid LICENSE_GUID_V8_COMMERCIAL = new Guid("868C63F5-3760-4A45-8600-5399CC57B47C");

	public static readonly Guid LICENSE_GUID_V6_WIP = new Guid("06BB1079-5A56-47A1-AD6D-0B45183D894B");

	public static readonly Guid LICENSE_GUID_THIS_VERSION = LICENSE_GUID_V8_COMMERCIAL;

	public static readonly Guid LICENSE_GUID_PREV_VERSION = LICENSE_GUID_V7_COMMERCIAL;

	public static string RegistryRoot => $"Software\\McNeel\\Rhinoceros\\{8}.0";
}
namespace System.Security.Cryptography
{
	public static class RhinoProtectedData
	{
		public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ProtectedData.Unprotect(encryptedData, optionalEntropy, scope);
		}

		public static byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ProtectedData.Protect(userData, optionalEntropy, scope);
		}
	}
}
namespace Eto.Forms
{
	public class AutoSizingDialog<T> : Dialog<T>
	{
		private Panel contentPanel;

		private Scrollable scrollable;

		private bool isResizing;

		private bool hasCustomSize;

		private bool hasShown;

		private Size? lastContentSize;

		public new Size ClientSize
		{
			get
			{
				return contentPanel.Size;
			}
			set
			{
				contentPanel.Size = value;
			}
		}

		public new Control Content
		{
			get
			{
				return contentPanel.Content;
			}
			set
			{
				contentPanel.Content = value;
			}
		}

		public AutoSizingDialog()
		{
			contentPanel = new Panel();
			contentPanel.SizeChanged += contentPanel_SizeChanged;
			scrollable = new Scrollable
			{
				Border = BorderType.None,
				ExpandContentWidth = false,
				ExpandContentHeight = false,
				Content = contentPanel
			};
			base.Content = scrollable;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			hasShown = true;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (hasShown && !isResizing && base.Resizable)
			{
				hasCustomSize = true;
				scrollable.ExpandContentWidth = true;
				scrollable.ExpandContentHeight = true;
			}
			isResizing = false;
		}

		private void contentPanel_SizeChanged(object sender, EventArgs e)
		{
			if (base.Loaded && !hasCustomSize)
			{
				Size size = scrollable.Content.Size;
				Size value = size;
				Size? size2 = lastContentSize;
				if (value != size2)
				{
					isResizing = true;
					base.ClientSize = size;
					lastContentSize = size;
				}
			}
		}
	}
}
namespace McNeel.RhinoAccounts
{
	internal class CacheEntry
	{
		public string ClientId { get; set; }

		public byte[] SerializedAssembly { get; set; }

		public OAuth2Token OAuth2Token { get; set; }

		public OpenIDConnectToken OpenIDConnectToken { get; set; }

		public void SetAssembly(Assembly assembly)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, assembly.FullName);
			SerializedAssembly = memoryStream.ToArray();
		}

		public Assembly GetAssembly()
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream serializationStream = new MemoryStream(SerializedAssembly);
			try
			{
				if (binaryFormatter.Deserialize(serializationStream) is string assemblyString)
				{
					return Assembly.Load(assemblyString);
				}
				return null;
			}
			catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is FileLoadException)
			{
				return null;
			}
		}
	}
	public class OAuth2Token : IOAuth2Token
	{
		public DateTime? Exp { get; private set; }

		public IReadOnlyCollection<string> Scope { get; private set; }

		public string RawToken { get; private set; }

		[JsonIgnore]
		public bool IsExpired
		{
			get
			{
				if (Exp.HasValue)
				{
					return Exp < DateTime.UtcNow;
				}
				return false;
			}
		}

		[JsonConstructor]
		public OAuth2Token(string rawToken, DateTime? exp, IReadOnlyCollection<string> scope)
		{
			Exp = exp;
			Scope = scope;
			RawToken = rawToken;
		}

		public OAuth2Token(string rawToken, int? secondsToExpiration = null, string scope = null)
			: this(rawToken, (!secondsToExpiration.HasValue) ? ((DateTime?)null) : new DateTime?(DateTime.UtcNow + TimeSpan.FromSeconds(secondsToExpiration.Value)), new ReadOnlyCollection<string>((scope == null) ? ((IList<string>)new string[0]) : ((IList<string>)scope.Split(new char[1] { ' ' }))))
		{
		}

		public override bool Equals(object obj)
		{
			if (obj is OAuth2Token oAuth2Token && object.Equals(RawToken, oAuth2Token.RawToken) && Scope != null && oAuth2Token.Scope != null)
			{
				return new HashSet<string>(Scope).SetEquals(oAuth2Token.Scope);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = RawToken.GetHashCode();
			if (Exp.HasValue)
			{
				num ^= Exp.GetHashCode();
			}
			if (Scope != null)
			{
				num ^= string.Join("", Scope.OrderBy((string x) => x)).GetHashCode();
			}
			return num;
		}
	}
	public class OpenIDConnectToken : IOpenIDConnectToken
	{
		public string Sub { get; private set; }

		public IReadOnlyDictionary<string, RhinoAccountsGroup> MemberGroups { get; private set; }

		public IReadOnlyDictionary<string, RhinoAccountsGroup> AdminGroups { get; private set; }

		public IReadOnlyDictionary<string, RhinoAccountsGroup> OwnerGroups { get; private set; }

		[JsonIgnore]
		public IReadOnlyDictionary<string, RhinoAccountsGroup> AllGroups => new ReadOnlyDictionary<string, RhinoAccountsGroup>(MemberGroups.ToList().Concat(AdminGroups.ToList()).Concat(OwnerGroups.ToList())
			.ToDictionary((KeyValuePair<string, RhinoAccountsGroup> x) => x.Key, (KeyValuePair<string, RhinoAccountsGroup> x) => x.Value));

		public DateTime? UpdatedAt { get; private set; }

		[JsonIgnore]
		public DateTime? Iat { get; private set; }

		[JsonIgnore]
		public DateTime? Exp { get; private set; }

		[JsonIgnore]
		public string Iss { get; private set; }

		[JsonIgnore]
		public string Aud { get; private set; }

		[JsonIgnore]
		public DateTime? AuthTime { get; private set; }

		[JsonIgnore]
		public string Nonce { get; private set; }

		[JsonIgnore]
		public string AtHash { get; private set; }

		public IReadOnlyCollection<string> Emails { get; private set; }

		public bool? EmailVerified { get; private set; }

		public string Name { get; private set; }

		public string Phone { get; private set; }

		public string Locale { get; private set; }

		public string Picture { get; private set; }

		public string RawToken { get; private set; }

		[JsonIgnore]
		public bool IsExpired
		{
			get
			{
				if (Exp.HasValue)
				{
					return Exp < DateTime.UtcNow;
				}
				return false;
			}
		}

		[JsonIgnore]
		public bool IsUpdated => !object.Equals(Iat, UpdatedAt);

		private DateTime _DateTimeFromUnixTime(long unixTime)
		{
			return new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(unixTime).UtcDateTime;
		}

		private IDictionary<object, object> _ValidateJWT(string encodedJWT, object key, bool validateLifetime = true, string validIssuer = null, string validAudience = null, string validJTI = null)
		{
			if (key == null)
			{
				return JWT.Payload<Dictionary<object, object>>(encodedJWT);
			}
			if (key is string)
			{
				key = Encoding.UTF8.GetBytes((string)key);
			}
			IDictionary<object, object> dictionary = JWT.Decode<Dictionary<object, object>>(encodedJWT, key);
			try
			{
				if (validateLifetime && ((dictionary.ContainsKey("nbf") && _DateTimeFromUnixTime((long)dictionary["nbf"]) > DateTime.UtcNow) || (dictionary.ContainsKey("exp") && _DateTimeFromUnixTime((long)dictionary["exp"]) < DateTime.UtcNow)))
				{
					throw new JoseException("JWT lifetime validation failed.");
				}
				if ((validIssuer != null && dictionary["iss"].ToString() != validIssuer) || (validAudience != null && dictionary["aud"].ToString() != validAudience) || (validJTI != null && dictionary["jti"].ToString() != validJTI))
				{
					throw new JoseException("JWT validation failed. 'iss', 'aud', or 'jti' do not match");
				}
			}
			catch (KeyNotFoundException innerException)
			{
				throw new JoseException("JWT Missing claims required for validation", innerException);
			}
			return dictionary;
		}

		private Tuple<ReadOnlyDictionary<string, RhinoAccountsGroup>, ReadOnlyDictionary<string, RhinoAccountsGroup>, ReadOnlyDictionary<string, RhinoAccountsGroup>> _ParseGroups(IDictionary<object, object> payload)
		{
			Dictionary<string, RhinoAccountsGroup> dictionary = new Dictionary<string, RhinoAccountsGroup>();
			Dictionary<string, RhinoAccountsGroup> dictionary2 = new Dictionary<string, RhinoAccountsGroup>();
			Dictionary<string, RhinoAccountsGroup> dictionary3 = new Dictionary<string, RhinoAccountsGroup>();
			if (payload.ContainsKey("com.rhino3d.accounts.member_groups"))
			{
				foreach (JToken item in (IEnumerable<JToken>)(JToken)payload["com.rhino3d.accounts.member_groups"])
				{
					if (item.Value<JToken>("id") != null && item.Value<JToken>("name") != null)
					{
						dictionary[item["id"].ToString()] = item.ToObject<RhinoAccountsGroup>();
					}
				}
			}
			if (payload.ContainsKey("com.rhino3d.accounts.admin_groups"))
			{
				foreach (JToken item2 in (IEnumerable<JToken>)(JToken)payload["com.rhino3d.accounts.admin_groups"])
				{
					if (item2.Value<JToken>("id") != null && item2.Value<JToken>("name") != null)
					{
						dictionary2[item2["id"].ToString()] = item2.ToObject<RhinoAccountsGroup>();
					}
				}
			}
			if (payload.ContainsKey("com.rhino3d.accounts.owner_groups"))
			{
				foreach (JToken item3 in (IEnumerable<JToken>)(JToken)payload["com.rhino3d.accounts.owner_groups"])
				{
					if (item3.Value<JToken>("id") != null && item3.Value<JToken>("name") != null)
					{
						dictionary3[item3["id"].ToString()] = item3.ToObject<RhinoAccountsGroup>();
					}
				}
			}
			return Tuple.Create(new ReadOnlyDictionary<string, RhinoAccountsGroup>(dictionary), new ReadOnlyDictionary<string, RhinoAccountsGroup>(dictionary2), new ReadOnlyDictionary<string, RhinoAccountsGroup>(dictionary3));
		}

		private ReadOnlyCollection<string> _ParseEmails(IDictionary<object, object> payload)
		{
			List<string> list = new List<string>();
			if (payload.ContainsKey("com.rhino3d.accounts.emails"))
			{
				foreach (JToken item in (IEnumerable<JToken>)(JToken)payload["com.rhino3d.accounts.emails"])
				{
					list.Add(item.ToString());
				}
			}
			else if (payload.ContainsKey("email"))
			{
				list.Add(payload["email"].ToString());
			}
			return new ReadOnlyCollection<string>(list);
		}

		private string _ParseLocale(IDictionary<object, object> payload)
		{
			string text = null;
			if (payload.ContainsKey("address") && ((JToken)payload["address"]).Value<JToken>("country") != null)
			{
				text = ((JToken)payload["address"])["country"].ToString();
			}
			string text2 = ((!payload.ContainsKey("locale")) ? null : payload["locale"]?.ToString());
			if (text2 != null)
			{
				if (text2.Length == 2 && text != null && text.Length == 2)
				{
					return text2.ToLower() + "-" + text.ToUpper();
				}
				return text2.ToLower();
			}
			return null;
		}

		[JsonConstructor]
		public OpenIDConnectToken(string name, string phone, string locale, string picture, bool? emailVerified, IReadOnlyCollection<string> emails, IReadOnlyDictionary<string, RhinoAccountsGroup> memberGroups, IReadOnlyDictionary<string, RhinoAccountsGroup> adminGroups, IReadOnlyDictionary<string, RhinoAccountsGroup> ownerGroups, string rawToken, DateTime? updatedAt)
			: this(rawToken)
		{
			if (name != null)
			{
				Name = name;
			}
			if (phone != null)
			{
				Phone = phone;
			}
			if (locale != null)
			{
				Locale = locale;
			}
			if (picture != null)
			{
				Picture = picture;
			}
			if (emailVerified.HasValue)
			{
				EmailVerified = emailVerified;
			}
			if (emails != null)
			{
				Emails = emails;
			}
			if (memberGroups != null)
			{
				MemberGroups = memberGroups;
			}
			if (adminGroups != null)
			{
				AdminGroups = adminGroups;
			}
			if (ownerGroups != null)
			{
				OwnerGroups = ownerGroups;
			}
			if (updatedAt.HasValue)
			{
				UpdatedAt = updatedAt;
			}
		}

		public OpenIDConnectToken(string rawToken, IDictionary<object, object> updatedUserInfo = null)
		{
			IDictionary<object, object> dictionary;
			try
			{
				dictionary = _ValidateJWT(rawToken, null, validateLifetime: false);
			}
			catch (Exception ex) when (ex is FormatException || ex is JoseException)
			{
				throw new RhinoAccountsInvalidTokenException(ex);
			}
			Sub = ((!dictionary.ContainsKey("sub")) ? null : dictionary["sub"]?.ToString());
			Iat = (dictionary.ContainsKey("iat") ? new DateTime?(_DateTimeFromUnixTime((long)dictionary["iat"])) : ((DateTime?)null));
			Exp = (dictionary.ContainsKey("exp") ? new DateTime?(_DateTimeFromUnixTime((long)dictionary["exp"])) : ((DateTime?)null));
			Iss = ((!dictionary.ContainsKey("iss")) ? null : dictionary["iss"]?.ToString());
			Aud = ((!dictionary.ContainsKey("aud")) ? null : dictionary["aud"]?.ToString());
			AuthTime = (dictionary.ContainsKey("auth_time") ? new DateTime?(_DateTimeFromUnixTime((long)dictionary["auth_time"])) : ((DateTime?)null));
			Nonce = ((!dictionary.ContainsKey("nonce")) ? null : dictionary["nonce"]?.ToString());
			AtHash = ((!dictionary.ContainsKey("at_hash")) ? null : dictionary["at_hash"]?.ToString());
			RawToken = rawToken;
			if (updatedUserInfo == null)
			{
				updatedUserInfo = dictionary;
				UpdatedAt = Iat;
			}
			else
			{
				UpdatedAt = DateTime.UtcNow;
			}
			Tuple<ReadOnlyDictionary<string, RhinoAccountsGroup>, ReadOnlyDictionary<string, RhinoAccountsGroup>, ReadOnlyDictionary<string, RhinoAccountsGroup>> tuple = _ParseGroups(updatedUserInfo);
			MemberGroups = new ReadOnlyDictionary<string, RhinoAccountsGroup>(tuple.Item1);
			AdminGroups = new ReadOnlyDictionary<string, RhinoAccountsGroup>(tuple.Item2);
			OwnerGroups = new ReadOnlyDictionary<string, RhinoAccountsGroup>(tuple.Item3);
			Name = ((!updatedUserInfo.ContainsKey("name")) ? null : updatedUserInfo["name"]?.ToString());
			EmailVerified = (updatedUserInfo.ContainsKey("email_verified") ? (updatedUserInfo["email_verified"] as bool?) : ((bool?)null));
			Emails = _ParseEmails(updatedUserInfo);
			Phone = ((!updatedUserInfo.ContainsKey("phone")) ? null : updatedUserInfo["phone"]?.ToString());
			Picture = ((!updatedUserInfo.ContainsKey("picture")) ? null : updatedUserInfo["picture"]?.ToString());
			Locale = _ParseLocale(updatedUserInfo);
		}

		public override bool Equals(object obj)
		{
			if (obj is OpenIDConnectToken openIDConnectToken && object.Equals(openIDConnectToken.RawToken, RawToken) && object.Equals(Name, openIDConnectToken.Name) && object.Equals(Phone, openIDConnectToken.Phone) && object.Equals(Locale, openIDConnectToken.Locale) && object.Equals(Picture, openIDConnectToken.Picture) && new HashSet<string>(Emails).SetEquals(openIDConnectToken.Emails))
			{
				return new HashSet<string>(AllGroups.Keys).SetEquals(openIDConnectToken.AllGroups.Keys);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = RawToken.GetHashCode();
			if (Name != null)
			{
				num ^= Name.GetHashCode();
			}
			if (Phone != null)
			{
				num ^= Phone.GetHashCode();
			}
			if (Locale != null)
			{
				num ^= Locale.GetHashCode();
			}
			if (Picture != null)
			{
				num ^= Picture.GetHashCode();
			}
			num ^= string.Join("", Emails.OrderBy((string x) => x)).GetHashCode();
			return num ^ string.Join("", AllGroups.Keys.OrderBy((string x) => x)).GetHashCode();
		}
	}
	public class PlatformServiceLocator : IPlatformServiceLocator
	{
		public T GetService<T>() where T : class
		{
			return new RhinoAccountsManagerSingleton() as T;
		}
	}
	public class ProgressModal : AutoSizingDialog<bool>
	{
		public Label MessageLabel { get; set; }

		public TextArea URLLabel { get; set; }

		public Button BrowserDidntOpenButton { get; set; }

		public Button CancelButton { get; set; }

		public Button CopyButton { get; set; }

		public Button NoButton { get; set; }

		public Button YesButton { get; set; }

		public StackLayout NotLoggedInLayout { get; set; }

		public StackLayout ProgressBarLayout { get; set; }

		public ProgressModal(string message, bool centerOnScreen = true)
		{
			base.ClientSize = new Size(520, -1);
			if (centerOnScreen)
			{
				RectangleF bounds = base.Screen.Bounds;
				base.Location = new Point((int)bounds.MiddleX - base.ClientSize.Width / 2, (int)bounds.MiddleY - base.ClientSize.Height / 2);
			}
			CancelButton = new Button();
			CancelButton.Text = Localization.LocalizeString("Cancel", 11);
			CopyButton = new Button
			{
				Visible = false
			};
			CopyButton.Click += delegate
			{
				new Clipboard().Text = URLLabel.Text;
			};
			CopyButton.Text = Localization.LocalizeString("Copy URL", 12);
			BrowserDidntOpenButton = new Button
			{
				Text = Localization.LocalizeString("My Browser Didn't Open...", 13),
				Visible = false
			};
			MessageLabel = new Label
			{
				Text = message
			};
			URLLabel = new TextArea
			{
				BackgroundColor = new Color(0f, 0f, 0f, 0f),
				Visible = false,
				ReadOnly = true
			};
			Button button;
			Button browserDidntOpenButton;
			Button button2;
			if (Application.Instance.Platform.IsMac)
			{
				button = CancelButton;
				browserDidntOpenButton = BrowserDidntOpenButton;
				button2 = CopyButton;
			}
			else
			{
				button = CopyButton;
				browserDidntOpenButton = BrowserDidntOpenButton;
				button2 = CancelButton;
			}
			ProgressBarLayout = new StackLayout
			{
				ClientSize = new Size(-1, -1),
				Orientation = Orientation.Vertical,
				Padding = 10,
				Spacing = 10,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				Items = 
				{
					(StackLayoutItem)new ProgressBar
					{
						Indeterminate = true
					},
					new StackLayoutItem(MessageLabel),
					new StackLayoutItem(URLLabel, expand: true),
					(StackLayoutItem)new StackLayout
					{
						Padding = 0,
						Spacing = 10,
						Orientation = Orientation.Horizontal,
						Items = 
						{
							(StackLayoutItem)null,
							(StackLayoutItem)button,
							(StackLayoutItem)browserDidntOpenButton,
							(StackLayoutItem)button2
						}
					}
				}
			};
			YesButton = new Button();
			YesButton.Text = Localization.LocalizeString("Yes", 14);
			NoButton = new Button();
			NoButton.Text = Localization.LocalizeString("No", 15);
			Button button3;
			Button button4;
			if (Application.Instance.Platform.IsMac)
			{
				button3 = NoButton;
				button4 = YesButton;
			}
			else
			{
				button3 = YesButton;
				button4 = NoButton;
			}
			Icon icon = Assets.Rhino.Eto.Icons.TryGet(ResourceIds.QuestionPng);
			NotLoggedInLayout = new StackLayout
			{
				ClientSize = new Size(-1, -1),
				Orientation = Orientation.Vertical,
				Padding = 10,
				Spacing = 10,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				Items = 
				{
					(StackLayoutItem)new StackLayout
					{
						Orientation = Orientation.Horizontal,
						Padding = 20,
						Spacing = 20,
						VerticalContentAlignment = VerticalAlignment.Stretch,
						Items = 
						{
							(StackLayoutItem)icon,
							(StackLayoutItem)new Label
							{
								Text = Localization.LocalizeString("You are not currently logged in to your Rhino Account. Would you like to log in now?", 16),
								Wrap = WrapMode.Word,
								Size = new Size(400, -1)
							}
						}
					},
					(StackLayoutItem)new StackLayout
					{
						Padding = 0,
						Spacing = 10,
						Orientation = Orientation.Horizontal,
						Items = 
						{
							(StackLayoutItem)null,
							(StackLayoutItem)button3,
							(StackLayoutItem)button4
						}
					}
				}
			};
			SwitchToProgressLayout();
		}

		public void SwitchToNotLoggedInLayout()
		{
			base.Content = NotLoggedInLayout;
			base.AbortButton = NoButton;
			base.DefaultButton = YesButton;
			base.Topmost = true;
		}

		public void SwitchToProgressLayout()
		{
			base.Content = ProgressBarLayout;
			base.AbortButton = CancelButton;
			base.DefaultButton = null;
			base.Topmost = false;
		}
	}
	public static class InternalRhinoAccountsManager
	{
		private static readonly byte[] _salt = Encoding.UTF8.GetBytes("I-BJOWEz");

		private static readonly string[] _redirectURIs = new string[6] { "http://127.0.0.1:1717/", "http://127.0.0.1:1718/", "http://127.0.0.1:1721/", "http://localhost:1717/", "http://localhost:1718/", "http://localhost:1721/" };

		private static readonly object _actionLock = new object();

		private static MethodBase _action;

		private static SecretKey _secretKey;

		private const string APP_COMPANY = "McNeel";

		private const string APP_NAME = "Rhinoceros";

		private const string APP_VERSION = "6.0";

		private const string APP_TITLE = "License Manager";

		private const string APP_FOLDER = "Licenses";

		public const string RhinoAccountsAuthEndpoint = "https://accounts.rhino3d.com/oauth2/auth";

		public const string RhinoAccountsTokenEndpoint = "https://accounts.rhino3d.com/oauth2/token";

		public const string RhinoAccountsUserInfoEndpoint = "https://accounts.rhino3d.com/oauth2/userinfo";

		public const string RhinoAccountsRevokeEndpoint = "https://accounts.rhino3d.com/oauth2/revoke";

		private static string _StoragePath
		{
			get
			{
				try
				{
					return Path.Combine(FileSettings.GetDataFolder(currentUser: true), "ra_cache.txt");
				}
				catch
				{
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "McNeel", "ra_cache.txt");
				}
			}
		}

		public static string LicenseFolder()
		{
			StringBuilder stringBuilder = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			stringBuilder.AppendFormat("{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}", Path.DirectorySeparatorChar, "McNeel", "Rhinoceros", "6.0", "License Manager", "Licenses");
			return stringBuilder.ToString();
		}

		private static byte[] _GenerateRandomBytes(ushort length)
		{
			using RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
			byte[] array = new byte[length];
			rNGCryptoServiceProvider.GetNonZeroBytes(array);
			return array;
		}

		private static string _GenerateRandomURLSafeString(ushort length)
		{
			return Convert.ToBase64String(_GenerateRandomBytes(length), Base64FormattingOptions.None).TrimEnd(new char[1] { '=' }).Replace('+', '-')
				.Replace('/', '_')
				.Substring(0, length);
		}

		private static string _GetLanguageId(CultureInfo cultureInfo = null)
		{
			if (cultureInfo == null)
			{
				cultureInfo = Thread.CurrentThread.CurrentUICulture;
			}
			RegionInfo regionInfo = null;
			try
			{
				if (!cultureInfo.IsNeutralCulture && cultureInfo.LCID != CultureInfo.InvariantCulture.LCID && cultureInfo.LCID != 4096)
				{
					regionInfo = new RegionInfo(cultureInfo.LCID);
				}
			}
			catch (Exception ex)
			{
				Rhino.Runtime.HostUtils.DebugString(ex?.ToString());
			}
			string text = regionInfo?.TwoLetterISORegionName?.ToUpper();
			string text2 = cultureInfo.TwoLetterISOLanguageName?.ToLower();
			return ((text2 == null) ? "en" : text2) + ((text == null) ? "" : ("-" + text));
		}

		private static StringWithQualityHeaderValue _AcceptLanguageHeader(CultureInfo cultureInfo = null)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new StringWithQualityHeaderValue(_GetLanguageId(cultureInfo));
		}

		private static AuthenticationHeaderValue _BasicAuthenticationHeader(string username, string password)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));
		}

		private static void _OpenResourceWithSystem(string resource)
		{
			Task.Run(delegate
			{
				Process.Start(new ProcessStartInfo(resource)
				{
					UseShellExecute = true
				});
			});
		}

		private static string _ToQueryString(NameValueCollection nvc)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string key in nvc.Keys)
			{
				if (string.IsNullOrEmpty(key))
				{
					continue;
				}
				string[] values = nvc.GetValues(key);
				if (values != null)
				{
					string[] array = values;
					foreach (string stringToEscape in array)
					{
						stringBuilder.Append((stringBuilder.Length == 0) ? "" : "&");
						stringBuilder.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(stringToEscape));
					}
				}
			}
			return stringBuilder.ToString();
		}

		private static List<CacheEntry> _ProcessReadCacheEntries(string encryptedEntriesAsString)
		{
			byte[] bytes = RhinoProtectedData.Unprotect(Convert.FromBase64String(encryptedEntriesAsString), _salt, (DataProtectionScope)0);
			return JsonConvert.DeserializeObject<List<CacheEntry>>(Encoding.UTF8.GetString(bytes));
		}

		private static List<CacheEntry> _ReadCachedEntriesFromDisk(string storagePath)
		{
			try
			{
				return _ProcessReadCacheEntries(File.ReadAllText(storagePath, Encoding.UTF8));
			}
			catch (Exception ex) when (ex is IOException || ex is JsonException || ex is CryptographicException || ex is ArgumentNullException || ex is FormatException)
			{
				return new List<CacheEntry>();
			}
		}

		private static async Task<List<CacheEntry>> _ReadCachedEntriesFromDiskAsync(string storagePath)
		{
			try
			{
				using StreamReader sr = new StreamReader(storagePath, Encoding.UTF8);
				return _ProcessReadCacheEntries(await sr.ReadToEndAsync().ConfigureAwait(continueOnCapturedContext: false));
			}
			catch (Exception ex) when (ex is IOException || ex is JsonException || ex is CryptographicException || ex is ArgumentNullException || ex is FormatException)
			{
				return new List<CacheEntry>();
			}
		}

		private static async Task _WriteCachedEntriesToDiskAsync(string storagePath, List<CacheEntry> cachedEntries)
		{
			string s = JsonConvert.SerializeObject(cachedEntries);
			string value = Convert.ToBase64String(RhinoProtectedData.Protect(Encoding.UTF8.GetBytes(s), _salt, (DataProtectionScope)0));
			using StreamWriter sw = new StreamWriter(storagePath, append: false, Encoding.UTF8);
			await sw.WriteAsync(value).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static Assembly _GetCallingAssembly(SecretKey secretKey)
		{
			MethodBase methodBase = null;
			SecretKey secretKey2 = null;
			lock (_actionLock)
			{
				methodBase = _action;
				secretKey2 = _secretKey;
			}
			if (methodBase == null)
			{
				throw new InvalidOperationException("The method you are trying to call in " + typeof(InternalRhinoAccountsManager).Name + " must be called within the protected code passed to " + typeof(InternalRhinoAccountsManager).Name + ".ExecuteProtectedCodeAsync");
			}
			if (!secretKey2.Equals(secretKey))
			{
				throw new InvalidOperationException("The secret key passed is incorrect. You must pass the secret key passed to your protected code in " + typeof(InternalRhinoAccountsManager).Name + ".ExecuteProtectedCodeAsync");
			}
			return methodBase.DeclaringType.Assembly;
		}

		public static RhinoAccountsServerException CreateRhinoAccountsServerException(string rawResponse)
		{
			RhinoAccountsServerException ex;
			try
			{
				Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawResponse);
				if (dictionary == null)
				{
					throw new JsonException("Response is null");
				}
				ex = ((!dictionary.ContainsKey("error_description")) ? new RhinoAccountsServerException() : new RhinoAccountsServerException(dictionary["error_description"]?.ToString()));
				foreach (KeyValuePair<string, object> item in dictionary)
				{
					if (item.Value != null)
					{
						ex.Data[item.Key] = (item.Value.GetType().IsSerializable ? item.Value : item.Value.ToString());
					}
					else
					{
						ex.Data[item.Key] = null;
					}
				}
			}
			catch (JsonException)
			{
				ex = new RhinoAccountsServerException();
			}
			return ex;
		}

		public static async Task ExecuteProtectedCodeAsync(Func<SecretKey, Task> protectedCode)
		{
			SecretKey secretKey = new SecretKey();
			lock (_actionLock)
			{
				if (_action != null)
				{
					throw new RhinoAccountsOperationInProgressException(_action.DeclaringType.Assembly);
				}
				_action = protectedCode.Method;
				_secretKey = secretKey;
			}
			try
			{
				await protectedCode(secretKey);
			}
			finally
			{
				lock (_actionLock)
				{
					_action = null;
					_secretKey = null;
				}
			}
		}

		public static void ExecuteProtectedCode(Action<SecretKey> protectedCode)
		{
			SecretKey secretKey = new SecretKey();
			lock (_actionLock)
			{
				if (_action != null)
				{
					throw new RhinoAccountsOperationInProgressException(_action.DeclaringType.Assembly);
				}
				_action = protectedCode.Method;
				_secretKey = secretKey;
			}
			try
			{
				protectedCode(secretKey);
			}
			finally
			{
				lock (_actionLock)
				{
					_action = null;
					_secretKey = null;
				}
			}
		}

		public static Tuple<OpenIDConnectToken, OAuth2Token> TryGetAuthTokens(string clientId, SecretKey secretKey)
		{
			return TryGetAuthTokens(clientId, null, secretKey);
		}

		public static Tuple<OpenIDConnectToken, OAuth2Token> TryGetAuthTokens(string clientId, IEnumerable<string> scope, SecretKey secretKey)
		{
			if (clientId == null)
			{
				throw new ArgumentNullException("clientId");
			}
			if (secretKey == null)
			{
				throw new ArgumentNullException("secretKey");
			}
			Assembly assembly = _GetCallingAssembly(secretKey);
			List<CacheEntry> list = _ReadCachedEntriesFromDisk(_StoragePath);
			HashSet<string> hashSet = ((scope == null) ? null : new HashSet<string>(scope));
			foreach (CacheEntry item in list)
			{
				if (assembly.Equals(item.GetAssembly()) && clientId == item.ClientId && (hashSet == null || hashSet.SetEquals(item.OAuth2Token.Scope)))
				{
					return Tuple.Create(item.OpenIDConnectToken, item.OAuth2Token);
				}
			}
			return null;
		}

		private static void RenderRedirectHTML(Uri redirectUri, HttpListenerContext context)
		{
			string s = string.Format("\r\n            <!doctype html>\r\n            <html>\r\n            <head>\r\n              <meta charset=\"utf-8\">\r\n              <title>Rhinoceros</title>\r\n            </head>\r\n            <body style=\"text-align: center; font-family: Lato, 'Helvetica Neue', Helvetica, Arial, sans-serif;\">\r\n\t            <p><a href='{0}' href>{1}</a></p>\r\n            </body>\r\n            <script>\r\n              //This delay is important.\r\n              setTimeout(function() {{ window.location.href = '{2}'; }}, 1000);\r\n            </script>\r\n            </html>\r\n          ", redirectUri, Localization.LocalizeString("Please click here to continue if you are not automatically redirected.", 17), redirectUri);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			context.Response.Headers.Add("Content-Type", "text/html");
			context.Response.ContentLength64 = bytes.Length;
			context.Response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		private static void RenderHTML(string message, HttpListenerContext context)
		{
			string s = "\r\n            <!doctype html>\r\n            <html>\r\n            <head>\r\n              <meta charset=\"utf-8\">\r\n              <title>Rhinoceros</title>\r\n              <style>\r\n  \t            @keyframes fadeIn {\r\n                    0% {opacity: 0.1;}\r\n                    100% {opacity: 1;}\r\n\t              }\r\n\t\r\n\t              .fadeIn {\r\n                      animation-duration: 5s;\r\n\t\t                  animation-name: fadeIn;\r\n\t              }\r\n              </style>\r\n            </head>\r\n            <body style=\"text-align: center; font-family: Lato, 'Helvetica Neue', Helvetica, Arial, sans-serif;\">\r\n\t            <div><img src=\"https://elisapi.mcneel.com/media/2\" alt=\"Rhinoceros\" style=\"width: 256px; height: 256px;\" class=\"fadeIn\"></div>\r\n\t            <p>" + message + "</p>\r\n            </body>\r\n            </html>\r\n          ";
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			context.Response.Headers.Add("Content-Type", "text/html");
			context.Response.ContentLength64 = bytes.Length;
			context.Response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		public static Task<Tuple<OpenIDConnectToken, OAuth2Token>> GetAuthTokensAsync(string clientId, string clientSecret, SecretKey secretKey, CancellationToken cancellationToken)
		{
			return GetAuthTokensAsync(clientId, clientSecret, new string[4] { "openid", "email", "profile", "groups" }, null, null, showUI: true, null, secretKey, cancellationToken);
		}

		public static Task<Tuple<OpenIDConnectToken, OAuth2Token>> GetAuthTokensAsync(string clientId, string clientSecret, IEnumerable<string> scope, string prompt, int? maxAge, bool showUI, IProgress<RhinoAccoountsProgressInfo> progress, SecretKey secretKey, CancellationToken cancellationToken)
		{
			return GetAuthTokensAsync(clientId, clientSecret, scope, prompt, maxAge, showUI, progress, null, keepListening: false, null, secretKey, cancellationToken);
		}

		public static async Task<Tuple<OpenIDConnectToken, OAuth2Token>> GetAuthTokensAsync(string clientId, string clientSecret, IEnumerable<string> scope, string prompt, int? maxAge, bool showUI, IProgress<RhinoAccoountsProgressInfo> progress, Func<Uri, Tuple<OpenIDConnectToken, OAuth2Token>, CancellationToken, Task<Uri>> redirectUriFn, bool keepListening, string userEmail, SecretKey secretKey, CancellationToken cancellationToken)
		{
			if (scope == null)
			{
				throw new ArgumentNullException("scope");
			}
			HashSet<string> hashSet = new HashSet<string>(scope);
			hashSet.Add("openid");
			scope = hashSet;
			if (clientId == null)
			{
				throw new ArgumentNullException("clientId");
			}
			if (clientSecret == null)
			{
				throw new ArgumentNullException("clientSecret");
			}
			if (secretKey == null)
			{
				throw new ArgumentNullException("secretKey");
			}
			Assembly callingAssembly = _GetCallingAssembly(secretKey);
			ProgressModal progressModal = null;
			CancellationTokenSource timedCts = new CancellationTokenSource(TimeSpan.FromMinutes(10.0));
			CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timedCts.Token);
			if (showUI)
			{
				Application.Instance.Invoke(delegate
				{
					progressModal = new ProgressModal(Localization.LocalizeString("Please Wait...", 9));
					progressModal.BrowserDidntOpenButton.Click += delegate
					{
						progressModal.BrowserDidntOpenButton.Visible = false;
						progressModal.URLLabel.Visible = true;
						progressModal.URLLabel.SelectAll();
						progressModal.CopyButton.Visible = true;
						progressModal.MessageLabel.Text = Localization.LocalizeString("Copy and paste the URL below into your browser:", 10);
					};
					progressModal.CancelButton.Click += delegate
					{
						timedCts.Cancel();
					};
				});
				Application.Instance.AsyncInvoke(delegate
				{
					progressModal.ShowModal(RhinoEtoApp.MainWindow);
				});
			}
			try
			{
				Tuple<OpenIDConnectToken, OAuth2Token> tokens = null;
				string chosenRedirectUri = null;
				string state = _GenerateRandomURLSafeString(64);
				IWebProxy proxy = WebRequest.DefaultWebProxy;
				IEnumerable<string> enumerable = _redirectURIs.Where((string x) => proxy == null || proxy.IsBypassed(new Uri(x)) || proxy.GetProxy(new Uri(x)) == null);
				if (enumerable.FirstOrDefault() == null)
				{
					throw new RhinoAccountsProxyException();
				}
				foreach (string item in enumerable)
				{
					if (AsyncHttpServer.CanBindToPrefix(item))
					{
						chosenRedirectUri = item;
						break;
					}
				}
				if (chosenRedirectUri == null)
				{
					throw new RhinoAccountsCannotListenException();
				}
				List<CacheEntry> entries = await _ReadCachedEntriesFromDiskAsync(_StoragePath).ConfigureAwait(continueOnCapturedContext: false);
				Func<HttpListenerContext, Task> responseFn = async delegate(HttpListenerContext context)
				{
					_ = 5;
					try
					{
						NameValueCollection nameValueCollection2 = HttpUtility.ParseQueryString(context.Request.Url.Query);
						if (nameValueCollection2.Get("state") != state)
						{
							RhinoAccountsInvalidStateException ex = new RhinoAccountsInvalidStateException();
							ex.Data["Request State"] = nameValueCollection2.Get("state");
							ex.Data["Inner State"] = state;
							throw ex;
						}
						if (nameValueCollection2.Get("error") != null)
						{
							RhinoAccountsException ex2 = new RhinoAccountsException();
							ex2.Data["Error in Query String"] = nameValueCollection2.Get("error");
							throw ex2;
						}
						RhinoAccoountsProgressInfo progressInfo3 = new RhinoAccoountsProgressInfo(ProgressState.RetrievingTokens);
						progress?.Report(progressInfo3);
						if (showUI)
						{
							Application.Instance.Invoke(delegate
							{
								progressModal.URLLabel.Text = "";
								progressModal.BrowserDidntOpenButton.Visible = false;
								progressModal.URLLabel.Visible = false;
								progressModal.CopyButton.Visible = false;
								progressModal.MessageLabel.Text = progressInfo3.Description;
							});
						}
						HttpClient httpClient = Proxy.CreateHttpClient();
						try
						{
							_ = 4;
							try
							{
								HttpContent content = (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[3]
								{
									new KeyValuePair<string, string>("code", nameValueCollection2.Get("code")),
									new KeyValuePair<string, string>("redirect_uri", chosenRedirectUri),
									new KeyValuePair<string, string>("grant_type", "authorization_code")
								});
								HttpRequestMessage val = new HttpRequestMessage(HttpMethod.Post, "https://accounts.rhino3d.com/oauth2/token");
								val.Headers.Authorization = _BasicAuthenticationHeader(clientId, clientSecret);
								val.Headers.AcceptLanguage.Add(_AcceptLanguageHeader());
								val.Content = content;
								HttpResponseMessage val2 = await ((HttpMessageInvoker)httpClient).SendAsync(val, cts.Token).ConfigureAwait(continueOnCapturedContext: false);
								if (!val2.IsSuccessStatusCode)
								{
									throw CreateRhinoAccountsServerException(await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
								}
								Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
								OpenIDConnectToken idToken = null;
								OAuth2Token oauth2Token = null;
								if (dictionary.ContainsKey("id_token"))
								{
									idToken = new OpenIDConnectToken(dictionary["id_token"].ToString());
								}
								if (dictionary.ContainsKey("access_token"))
								{
									oauth2Token = new OAuth2Token(dictionary["access_token"].ToString(), int.Parse(dictionary["expires_in"].ToString()), dictionary["scope"].ToString());
								}
								entries = await _ReadCachedEntriesFromDiskAsync(_StoragePath).ConfigureAwait(continueOnCapturedContext: false);
								string text = entries.FirstOrDefault()?.OpenIDConnectToken?.Sub;
								string text2 = entries.FirstOrDefault((CacheEntry x) => x.OpenIDConnectToken?.Name != null)?.OpenIDConnectToken?.Name;
								string text3 = entries.FirstOrDefault((CacheEntry x) => x.OpenIDConnectToken?.Emails?.FirstOrDefault() != null)?.OpenIDConnectToken?.Emails?.FirstOrDefault();
								if (text != null && idToken != null && text != idToken.Sub)
								{
									throw new RhinoAccountsAuthTokenMismatchException(text2 + " - " + text3, idToken?.Name + " - " + idToken?.Emails.FirstOrDefault());
								}
								foreach (CacheEntry item2 in new List<CacheEntry>(entries))
								{
									if (callingAssembly.Equals(item2.GetAssembly()) && clientId == item2.ClientId)
									{
										entries.Remove(item2);
									}
								}
								CacheEntry newEntry = new CacheEntry
								{
									ClientId = clientId,
									OAuth2Token = oauth2Token,
									OpenIDConnectToken = idToken
								};
								newEntry.SetAssembly(callingAssembly);
								entries.Add(newEntry);
								await _WriteCachedEntriesToDiskAsync(_StoragePath, entries).ConfigureAwait(continueOnCapturedContext: false);
								SendUserInformation(new UserIds
								{
									UserDeviceId = Analytics.UserId.ToString(),
									DeviceId = Rhino.Runtime.HostUtils.DeviceId.ToString(),
									RhinoAccountId = newEntry.OpenIDConnectToken.Sub,
									Emails = newEntry.OpenIDConnectToken.Emails.ToList()
								});
								tokens = new Tuple<OpenIDConnectToken, OAuth2Token>(idToken, oauth2Token);
							}
							catch (HttpRequestException ex3)
							{
								throw new RhinoAccountsServerNotReachableException((Exception)ex3);
							}
						}
						finally
						{
							((IDisposable)httpClient)?.Dispose();
						}
						if (redirectUriFn != null)
						{
							RenderRedirectHTML(await redirectUriFn(new Uri(chosenRedirectUri), tokens, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), context);
						}
						else
						{
							RenderHTML(Localization.LocalizeString("Login completed. Please go back to Rhino.", 1), context);
						}
					}
					catch (Exception ex4)
					{
						RenderHTML(string.Format(Localization.LocalizeString("Login error ({0}). Please go back to Rhino.", 18), ex4.Message), context);
						throw;
					}
				};
				Task task = AsyncHttpServer.ListenAsync(new string[1] { chosenRedirectUri }, responseFn, cancellationToken);
				NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
				nameValueCollection.Add("state", state);
				nameValueCollection.Add("response_type", "code");
				nameValueCollection.Add("client_id", clientId);
				nameValueCollection.Add("scope", string.Join(" ", scope));
				nameValueCollection.Add("redirect_uri", chosenRedirectUri);
				if (entries.Count > 0 && entries.First().OpenIDConnectToken != null)
				{
					nameValueCollection.Add("login_hint", entries.First().OpenIDConnectToken.Sub);
				}
				if (prompt != null)
				{
					nameValueCollection.Add("prompt", prompt);
				}
				if (maxAge.HasValue)
				{
					nameValueCollection.Add("max_age", maxAge.ToString());
				}
				if (userEmail != null)
				{
					nameValueCollection.Add("email", userEmail);
				}
				string authURL = "https://accounts.rhino3d.com/oauth2/auth?" + _ToQueryString(nameValueCollection);
				RhinoAccoountsProgressInfo progressInfo = new RhinoAccoountsProgressInfo(ProgressState.AwaitingLogin, new Dictionary<object, object> { ["auth_url"] = authURL });
				progress?.Report(progressInfo);
				if (showUI)
				{
					Application.Instance.Invoke(delegate
					{
						progressModal.URLLabel.Text = authURL;
						progressModal.BrowserDidntOpenButton.Visible = true;
						progressModal.URLLabel.Visible = false;
						progressModal.CopyButton.Visible = false;
						progressModal.MessageLabel.Text = progressInfo.Description;
					});
				}
				_OpenResourceWithSystem(authURL);
				await task;
				if (redirectUriFn != null && keepListening)
				{
					RhinoAccoountsProgressInfo progressInfo2 = new RhinoAccoountsProgressInfo(ProgressState.AwaitingRedirect);
					progress?.Report(progressInfo2);
					if (showUI)
					{
						Application.Instance.Invoke(delegate
						{
							progressModal.URLLabel.Text = "";
							progressModal.BrowserDidntOpenButton.Visible = false;
							progressModal.URLLabel.Visible = false;
							progressModal.CopyButton.Visible = false;
							progressModal.MessageLabel.Text = progressInfo2.Description;
						});
					}
					Func<HttpListenerContext, Task> responseFn2 = delegate(HttpListenerContext context)
					{
						RenderHTML(Localization.LocalizeString("Task completed. Please go back to Rhino", 19), context);
						Thread.Sleep(500);
						return Task.CompletedTask;
					};
					await AsyncHttpServer.ListenAsync(new string[1] { chosenRedirectUri }, responseFn2, cancellationToken);
				}
				PlatformServiceProvider.Service.BringCurrentProcessToForeground();
				return tokens;
			}
			catch (HttpListenerException innerException)
			{
				throw new RhinoAccountsCannotListenException(innerException);
			}
			finally
			{
				cts.Dispose();
				timedCts.Dispose();
				if (showUI)
				{
					Application.Instance.Invoke(delegate
					{
						progressModal.Close();
					});
				}
			}
		}

		public static async Task<OpenIDConnectToken> UpdateOpenIDConnectTokenAsync(OpenIDConnectToken currentToken, OAuth2Token oauth2Token, SecretKey secretKey, CancellationToken cancellationToken)
		{
			if (currentToken == null)
			{
				throw new ArgumentNullException("currentToken");
			}
			if (oauth2Token == null)
			{
				throw new ArgumentNullException("oauth2Token");
			}
			if (secretKey == null)
			{
				throw new ArgumentNullException("secretKey");
			}
			Assembly callingAssembly = _GetCallingAssembly(secretKey);
			using (SHA256CryptoServiceProvider sHA256CryptoServiceProvider = new SHA256CryptoServiceProvider())
			{
				if (Convert.ToBase64String(sHA256CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(oauth2Token.RawToken)).Take(16).ToArray()).Replace('+', '-').Replace('/', '_')
					.TrimEnd(new char[1] { '=' }) != currentToken.AtHash?.TrimEnd(new char[1] { '=' }))
				{
					throw new RhinoAccountsAuthTokenMismatchException("The OAuth2 token passed cannoot be used to update the OpenID Connect token passed; the at_hash property does not match.");
				}
			}
			try
			{
				HttpClient httpClient = Proxy.CreateHttpClient();
				try
				{
					HttpRequestMessage val = new HttpRequestMessage(HttpMethod.Get, "https://accounts.rhino3d.com/oauth2/userinfo");
					val.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oauth2Token.RawToken);
					val.Headers.AcceptLanguage.Add(_AcceptLanguageHeader());
					HttpResponseMessage val2 = await ((HttpMessageInvoker)httpClient).SendAsync(val, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (!val2.IsSuccessStatusCode)
					{
						throw CreateRhinoAccountsServerException(await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
					}
					IDictionary<object, object> updatedUserInfo = JsonConvert.DeserializeObject<IDictionary<object, object>>(await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
					OpenIDConnectToken newToken = new OpenIDConnectToken(currentToken.RawToken, updatedUserInfo);
					List<CacheEntry> list = await _ReadCachedEntriesFromDiskAsync(_StoragePath).ConfigureAwait(continueOnCapturedContext: false);
					CacheEntry cacheEntry = list.FirstOrDefault((CacheEntry x) => x.OpenIDConnectToken?.RawToken == currentToken.RawToken && callingAssembly.Equals(x.GetAssembly()));
					if (cacheEntry != null)
					{
						cacheEntry.OpenIDConnectToken = newToken;
						await _WriteCachedEntriesToDiskAsync(_StoragePath, list).ConfigureAwait(continueOnCapturedContext: false);
					}
					return newToken;
				}
				finally
				{
					((IDisposable)httpClient)?.Dispose();
				}
			}
			catch (HttpRequestException ex)
			{
				throw new RhinoAccountsServerNotReachableException((Exception)ex);
			}
			catch (JsonException innerException)
			{
				throw new RhinoAccountsInvalidResponseException(innerException);
			}
		}

		public static async Task RevokeAuthTokenAsync(OAuth2Token oauth2Token, SecretKey secretKey, CancellationToken cancellationToken)
		{
			if (oauth2Token == null)
			{
				throw new ArgumentNullException("oauth2Token");
			}
			if (secretKey == null)
			{
				throw new ArgumentNullException("secretKey");
			}
			Assembly callingAssembly = _GetCallingAssembly(secretKey);
			List<CacheEntry> entries = await _ReadCachedEntriesFromDiskAsync(_StoragePath).ConfigureAwait(continueOnCapturedContext: false);
			foreach (CacheEntry item in new List<CacheEntry>(entries))
			{
				if (callingAssembly.Equals(item.GetAssembly()) && oauth2Token.Equals(item.OAuth2Token))
				{
					entries.Remove(item);
				}
			}
			try
			{
				HttpClient httpClient = Proxy.CreateHttpClient();
				try
				{
					HttpContent content = (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[1]
					{
						new KeyValuePair<string, string>("token", oauth2Token.RawToken)
					});
					HttpRequestMessage val = new HttpRequestMessage(HttpMethod.Post, "https://accounts.rhino3d.com/oauth2/token");
					val.Headers.AcceptLanguage.Add(_AcceptLanguageHeader());
					val.Content = content;
					await ((HttpMessageInvoker)httpClient).SendAsync(val, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				finally
				{
					((IDisposable)httpClient)?.Dispose();
				}
			}
			catch (HttpRequestException)
			{
			}
			await _WriteCachedEntriesToDiskAsync(_StoragePath, entries).ConfigureAwait(continueOnCapturedContext: false);
		}

		public static async Task RevokeAllAuthTokensAsync(SecretKey secretKey, CancellationToken cancellationToken)
		{
			_GetCallingAssembly(secretKey);
			foreach (CacheEntry item in await _ReadCachedEntriesFromDiskAsync(_StoragePath).ConfigureAwait(continueOnCapturedContext: false))
			{
				try
				{
					HttpClient httpClient = Proxy.CreateHttpClient();
					try
					{
						HttpContent content = (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[1]
						{
							new KeyValuePair<string, string>("token", item.OAuth2Token.RawToken)
						});
						HttpRequestMessage val = new HttpRequestMessage(HttpMethod.Post, "https://accounts.rhino3d.com/oauth2/token");
						val.Headers.AcceptLanguage.Add(_AcceptLanguageHeader());
						val.Content = content;
						await ((HttpMessageInvoker)httpClient).SendAsync(val, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					finally
					{
						((IDisposable)httpClient)?.Dispose();
					}
				}
				catch (HttpRequestException)
				{
				}
			}
			await _WriteCachedEntriesToDiskAsync(_StoragePath, new List<CacheEntry>()).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static void SendIdInformation(UserIds ids)
		{
			try
			{
				string value = JsonConvert.SerializeObject(ids);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.mcneel.com/license/associate_device_and_account/");
				httpWebRequest.ContentType = "application/json";
				httpWebRequest.Accept = "application/json";
				httpWebRequest.Method = "POST";
				httpWebRequest.Timeout = 5000;
				CultureInfo cultureInfo = new CultureInfo(AppearanceSettings.LanguageIdentifier);
				httpWebRequest.Headers["Accept-Language"] = cultureInfo.Name;
				using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
				{
					streamWriter.Write(value);
				}
				using StreamReader streamReader = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream());
				streamReader.ReadToEnd();
			}
			catch
			{
			}
		}

		private static void SendUserInformation(UserIds userIds)
		{
			Task.Run(delegate
			{
				SendIdInformation(userIds);
			});
		}
	}
	internal class UserIds
	{
		public string UserDeviceId { get; set; }

		public string DeviceId { get; set; }

		public string RhinoAccountId { get; set; }

		public List<string> Emails { get; set; }
	}
	public class RhinoAccountsManagerSingleton : IRhinoAccountsManager
	{
		public void ExecuteProtectedCode(Action<SecretKey> protectedCode)
		{
			InternalRhinoAccountsManager.ExecuteProtectedCode(protectedCode);
		}

		public Task ExecuteProtectedCodeAsync(Func<SecretKey, Task> protectedCode)
		{
			return InternalRhinoAccountsManager.ExecuteProtectedCodeAsync(protectedCode);
		}

		public async Task<Tuple<IOpenIDConnectToken, IOAuth2Token>> GetAuthTokensAsync(string clientId, string clientSecret, SecretKey secretKey, CancellationToken cancellationToken)
		{
			Tuple<OpenIDConnectToken, OAuth2Token> tuple = await InternalRhinoAccountsManager.GetAuthTokensAsync(clientId, clientSecret, secretKey, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return Tuple.Create((IOpenIDConnectToken)tuple.Item1, (IOAuth2Token)tuple.Item2);
		}

		public async Task<Tuple<IOpenIDConnectToken, IOAuth2Token>> GetAuthTokensAsync(string clientId, string clientSecret, IEnumerable<string> scope, string prompt, int? maxAge, bool showUI, IProgress<RhinoAccoountsProgressInfo> progress, SecretKey secretKey, CancellationToken cancellationToken)
		{
			Tuple<OpenIDConnectToken, OAuth2Token> tuple = await InternalRhinoAccountsManager.GetAuthTokensAsync(clientId, clientSecret, scope, prompt, maxAge, showUI, progress, secretKey, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return Tuple.Create((IOpenIDConnectToken)tuple.Item1, (IOAuth2Token)tuple.Item2);
		}

		public Task RevokeAuthTokenAsync(IOAuth2Token oauth2Token, SecretKey secretKey, CancellationToken cancellationToken)
		{
			return InternalRhinoAccountsManager.RevokeAuthTokenAsync((OAuth2Token)oauth2Token, secretKey, cancellationToken);
		}

		public Tuple<IOpenIDConnectToken, IOAuth2Token> TryGetAuthTokens(string clientId, SecretKey secretKey)
		{
			Tuple<OpenIDConnectToken, OAuth2Token> tuple = InternalRhinoAccountsManager.TryGetAuthTokens(clientId, secretKey);
			if (tuple == null)
			{
				return null;
			}
			return Tuple.Create((IOpenIDConnectToken)tuple.Item1, (IOAuth2Token)tuple.Item2);
		}

		public Tuple<IOpenIDConnectToken, IOAuth2Token> TryGetAuthTokens(string clientId, IEnumerable<string> scope, SecretKey secretKey)
		{
			Tuple<OpenIDConnectToken, OAuth2Token> tuple = InternalRhinoAccountsManager.TryGetAuthTokens(clientId, scope, secretKey);
			if (tuple == null)
			{
				return null;
			}
			return Tuple.Create((IOpenIDConnectToken)tuple.Item1, (IOAuth2Token)tuple.Item2);
		}

		public async Task<IOpenIDConnectToken> UpdateOpenIDConnectTokenAsync(IOpenIDConnectToken currentToken, IOAuth2Token oauth2Token, SecretKey secretKey, CancellationToken cancellationToken)
		{
			return await InternalRhinoAccountsManager.UpdateOpenIDConnectTokenAsync((OpenIDConnectToken)currentToken, (OAuth2Token)oauth2Token, secretKey, cancellationToken);
		}
	}
}
namespace McNeel.Network
{
	public static class AsyncHttpServer
	{
		private static bool PrefixPathMatces(ICollection<string> prefixes, Uri requestUrl)
		{
			foreach (string prefix in prefixes)
			{
				if (new Uri(prefix).AbsolutePath == requestUrl.AbsolutePath)
				{
					return true;
				}
			}
			return false;
		}

		public static async Task ListenAsync(ICollection<string> prefixes, Func<HttpListenerContext, Task> responseFn, CancellationToken cancelToken)
		{
			HttpListener listener = new HttpListener();
			try
			{
				foreach (string prefix in prefixes)
				{
					listener.Prefixes.Add(prefix);
				}
				listener.Start();
				HttpListenerContext httpListenerContext;
				do
				{
					Task task = Task.Delay(-1, cancelToken);
					Task<HttpListenerContext> contextTask = listener.GetContextAsync();
					await Task.WhenAny(new Task[2] { task, contextTask }).ConfigureAwait(continueOnCapturedContext: false);
					cancelToken.ThrowIfCancellationRequested();
					httpListenerContext = await contextTask.ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!PrefixPathMatces(prefixes, httpListenerContext.Request.Url));
				await responseFn(httpListenerContext).ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				listener.Stop();
				((IDisposable)listener).Dispose();
			}
		}

		public static bool CanBindToPrefix(string prefix)
		{
			try
			{
				HttpListener httpListener = new HttpListener();
				httpListener.Prefixes.Add(prefix);
				httpListener.Start();
				httpListener.Stop();
				return true;
			}
			catch (HttpListenerException)
			{
				return false;
			}
		}
	}
	internal class RhinoMainWindowHelper
	{
		public Window GetMainWindow()
		{
			return RhinoEtoApp.MainWindow;
		}
	}
	public static class Proxy
	{
		private class ProxyDelegatingHandler : DelegatingHandler
		{
			private HttpClientHandler _httpClientHandler;

			public ProxyDelegatingHandler(IWebProxy proxy)
				: base((HttpMessageHandler)new HttpClientHandler())
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Expected O, but got Unknown
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Expected O, but got Unknown
				_httpClientHandler = (HttpClientHandler)((DelegatingHandler)this).InnerHandler;
				_httpClientHandler.Proxy = proxy;
				_httpClientHandler.PreAuthenticate = true;
				_httpClientHandler.UseDefaultCredentials = false;
				_httpClientHandler.UseProxy = proxy != null;
			}

			public Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return ((DelegatingHandler)this).SendAsync(request, cancellationToken);
			}
		}

		public class AuthenticatedProxyHttpClientHandler : HttpMessageHandler
		{
			public uint ProxyAuthorizationRetryCount { get; private set; }

			public AuthenticatedProxyHttpClientHandler(uint proxyAuthorizationRetryCount = 2u)
			{
				ProxyAuthorizationRetryCount = proxyAuthorizationRetryCount;
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				if (!RhinoApp.IsInternetAccessAllowed)
				{
					throw new HttpRequestException("Internet Access Blocked by System Administrator. See HKEY_LOCAL_MACHINE\\Software\\McNeel\\DisableInternetAccess");
				}
				uint retryCount = ProxyAuthorizationRetryCount;
				Uri proxyUri = null;
				NetworkCredential credentials = null;
				IWebProxy proxy = WebRequest.DefaultWebProxy;
				if (_ShouldBypassProxy())
				{
					proxy = null;
				}
				else if (proxy != null && !proxy.IsBypassed(request.RequestUri))
				{
					proxyUri = proxy.GetProxy(request.RequestUri);
					if (proxyUri != null)
					{
						proxy = new WebProxy(proxyUri);
						credentials = await _TryGetProxyCredentialsAsync(cancellationToken, proxyUri, askUser: false, null).ConfigureAwait(continueOnCapturedContext: false);
						proxy.Credentials = credentials;
					}
				}
				ProxyDelegatingHandler proxyDelegatingHandler = new ProxyDelegatingHandler(proxy);
				Func<Task<HttpResponseMessage>> retryLoop = null;
				retryLoop = async delegate
				{
					HttpResponseMessage result2 = default(HttpResponseMessage);
					object obj;
					int num;
					try
					{
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
						HttpResponseMessage result = await proxyDelegatingHandler.SendHttpRequestAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (result.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
						{
							Trace.WriteLine("Before trying to get credentials (A)");
							credentials = await _TryGetProxyCredentialsAsync(cancellationToken, proxyUri, askUser: true, credentials).ConfigureAwait(continueOnCapturedContext: false);
							proxy.Credentials = credentials;
							Trace.WriteLine("After trying to get credentials (A)");
							if (proxy.Credentials != null)
							{
								proxyDelegatingHandler = new ProxyDelegatingHandler(proxy);
								retryCount--;
								result2 = await retryLoop().ConfigureAwait(continueOnCapturedContext: false);
								return result2;
							}
						}
						result2 = result;
						return result2;
					}
					catch (HttpRequestException ex)
					{
						HttpRequestException ex2 = ex;
						obj = ex2;
						num = 1;
					}
					if (num == 1)
					{
						HttpRequestException ex3 = (HttpRequestException)obj;
						Trace.WriteLine($"In catch hre: {ex3}");
						if (((Exception)(object)ex3).InnerException is WebException { Response: HttpWebResponse { StatusCode: HttpStatusCode.ProxyAuthenticationRequired } } && proxy != null && retryCount != 0)
						{
							Trace.WriteLine("Before trying to get credentials (B)");
							credentials = await _TryGetProxyCredentialsAsync(cancellationToken, proxyUri, askUser: true, credentials).ConfigureAwait(continueOnCapturedContext: false);
							proxy.Credentials = credentials;
							Trace.WriteLine("After trying to get credentials (B)");
							if (proxy.Credentials != null)
							{
								proxyDelegatingHandler = new ProxyDelegatingHandler(proxy);
								retryCount--;
								return await retryLoop().ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						ExceptionDispatchInfo.Capture((obj as Exception) ?? throw obj).Throw();
					}
					return result2;
				};
				return await retryLoop().ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public class AuthenticatedProxyWebSocket
		{
			private WebSocket _websocket;

			private bool _manuallyClosed;

			public WebSocketState ReadyState => _websocket.ReadyState;

			public event EventHandler<CloseEventArgs> OnClose;

			public event EventHandler<WebSocketSharp.ErrorEventArgs> OnError;

			public event EventHandler<MessageEventArgs> OnMessage;

			public event EventHandler OnOpen;

			public void Connect(CancellationToken cancelToken, string url, uint proxyAuthorizationRetryCount = 2u, params string[] protocols)
			{
				Uri uri = new Uri(url);
				_websocket = new WebSocket(url, protocols);
				_websocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
				uint retryCount = proxyAuthorizationRetryCount;
				Uri proxyUri = null;
				NetworkCredential credentials = null;
				IWebProxy proxy = WebRequest.DefaultWebProxy;
				if (_ShouldBypassProxy())
				{
					proxy = null;
				}
				else if (proxy != null && !proxy.IsBypassed(uri))
				{
					proxyUri = proxy.GetProxy(uri);
					if (proxyUri != null)
					{
						credentials = _TryGetProxyCredentialsAsync(cancelToken, proxyUri, askUser: false, null).GetAwaiter().GetResult();
						_websocket.SetProxy(proxyUri.ToString(), credentials?.UserName, credentials?.Password);
					}
				}
				Action retryLoop = null;
				retryLoop = delegate
				{
					_websocket.OnOpen += this.OnOpen;
					_websocket.OnMessage += this.OnMessage;
					_websocket.OnError += this.OnError;
					_websocket.OnClose += delegate(object sender, CloseEventArgs e)
					{
						//IL_014e: Unknown result type (might be due to invalid IL or missing references)
						//IL_015a: Expected O, but got Unknown
						//IL_0037: Unknown result type (might be due to invalid IL or missing references)
						//IL_003d: Expected O, but got Unknown
						if (!_manuallyClosed && e.Code == 1006 && proxy != null && retryCount != 0)
						{
							HttpClient val = new HttpClient();
							try
							{
								UriBuilder uriBuilder = new UriBuilder(uri);
								if (uri.Scheme == "wss")
								{
									uriBuilder.Scheme = "https";
								}
								else
								{
									uriBuilder.Scheme = "http";
								}
								try
								{
									if (val.GetAsync(uriBuilder.Uri, cancelToken).GetAwaiter().GetResult()
										.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
									{
										credentials = _TryGetProxyCredentialsAsync(cancelToken, proxyUri, askUser: true, credentials).GetAwaiter().GetResult();
										if (credentials != null)
										{
											_websocket = new WebSocket(url, protocols);
											_websocket.SetProxy(proxyUri.ToString(), credentials.UserName, credentials.Password);
											retryCount--;
											retryLoop();
											return;
										}
									}
								}
								catch (HttpRequestException ex)
								{
									if ((((int)ex != 0) ? ((Exception)ex).InnerException : null) is WebException { Response: HttpWebResponse { StatusCode: HttpStatusCode.ProxyAuthenticationRequired } })
									{
										credentials = _TryGetProxyCredentialsAsync(cancelToken, proxyUri, askUser: true, credentials).GetAwaiter().GetResult();
										if (credentials != null)
										{
											_websocket = new WebSocket(url, protocols);
											_websocket.SetProxy(proxyUri.ToString(), credentials.UserName, credentials.Password);
											retryCount--;
											retryLoop();
											return;
										}
									}
								}
							}
							finally
							{
								((IDisposable)val)?.Dispose();
							}
						}
						this.OnClose?.Invoke(sender, e);
					};
					_websocket.Connect();
				};
				retryLoop();
			}

			public void Close()
			{
				_manuallyClosed = true;
				_websocket.Close();
			}

			public void Send(string data)
			{
				_websocket.Send(data);
			}

			public void Send(byte[] data)
			{
				_websocket.Send(data);
			}

			public bool Ping()
			{
				return _websocket.Ping();
			}
		}

		private static object _credentialsLock = new object();

		private static bool? _cachedBypassProxy = null;

		private static NetworkCredential _cachedCredentials;

		private static Uri _cachedCredentialsProxyUri;

		private static readonly byte[] _salt = Encoding.UTF8.GetBytes("N-DLCMXU");

		private static ProxyDialog _proxyDialog;

		private static string _settingStorageFile
		{
			get
			{
				try
				{
					return Path.Combine(FileSettings.GetDataFolder(currentUser: true), "proxy.txt");
				}
				catch
				{
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "McNeel", "proxy.txt");
				}
			}
		}

		private static string _bypassProxyStorageFile
		{
			get
			{
				try
				{
					return Path.Combine(FileSettings.GetDataFolder(currentUser: true), "bypass_proxy.txt");
				}
				catch
				{
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "McNeel", "proxy.txt");
				}
			}
		}

		private static Task<Tuple<NetworkCredential, bool>> _AskForCredentialsAsync(CancellationToken cancelToken, Uri proxiUri)
		{
			Trace.WriteLine("Begin _AskForCredentials");
			TaskCompletionSource<Tuple<NetworkCredential, bool>> tcs = new TaskCompletionSource<Tuple<NetworkCredential, bool>>();
			Application.Instance.AsyncInvoke(delegate
			{
				try
				{
					NetworkCredential credentials = null;
					bool saveCredentials = false;
					if (_proxyDialog == null)
					{
						_proxyDialog = new ProxyDialog(proxiUri);
						_proxyDialog.Topmost = true;
						cancelToken.Register(delegate
						{
							Application.Instance.AsyncInvoke(delegate
							{
								_proxyDialog?.Close(result: false);
							});
						});
						Trace.WriteLine("Before ShowModal");
						bool flag = false;
						if (Rhino.Runtime.HostUtils.RunningInRhino)
						{
							Window mainWindow = new RhinoMainWindowHelper().GetMainWindow();
							flag = _proxyDialog.ShowModal(mainWindow);
						}
						else
						{
							flag = _proxyDialog.ShowModal();
						}
						Trace.WriteLine("After ShowModal");
						if (flag)
						{
							credentials = new NetworkCredential(_proxyDialog.Username.Text, _proxyDialog.Password.Text);
							saveCredentials = _proxyDialog.RememberCredentials.Checked.Value;
						}
						_proxyDialog = null;
						tcs.SetResult(Tuple.Create(credentials, saveCredentials));
					}
					else
					{
						_proxyDialog.Closed += delegate
						{
							if (_proxyDialog.Result)
							{
								credentials = new NetworkCredential(_proxyDialog.Username.Text, _proxyDialog.Password.Text);
								saveCredentials = _proxyDialog.RememberCredentials.Checked.Value;
							}
							tcs.SetResult(Tuple.Create(credentials, saveCredentials));
						};
					}
				}
				catch (Exception exception)
				{
					tcs.SetException(exception);
				}
			});
			Trace.WriteLine("End _AskCredentials");
			return tcs.Task;
		}

		private static bool _ShouldBypassProxy()
		{
			try
			{
				if (!_cachedBypassProxy.HasValue)
				{
					_cachedBypassProxy = File.Exists(_bypassProxyStorageFile);
				}
				return _cachedBypassProxy.Value;
			}
			catch (Exception ex) when (ex is IOException || ex is JsonException)
			{
				return false;
			}
		}

		private static Tuple<NetworkCredential, Uri> _ReadCredentialsFromDisk(string storagePath)
		{
			Trace.WriteLine("Begin _ReadCredentialsFromDisk");
			NetworkCredential item = null;
			Uri item2 = null;
			if (storagePath == null)
			{
				return Tuple.Create(item, item2);
			}
			try
			{
				Trace.WriteLine("Before ReadAllText");
				string value = File.ReadAllText(storagePath);
				Trace.WriteLine("After ReadAllText");
				ProxyFile proxyFile = JsonConvert.DeserializeObject<ProxyFile>(value);
				byte[] proxy = proxyFile.Proxy;
				byte[] username = proxyFile.Username;
				byte[] password = proxyFile.Password;
				Trace.WriteLine("Before Unprotect");
				item = new NetworkCredential(Encoding.UTF8.GetString(RhinoProtectedData.Unprotect(username, _salt, (DataProtectionScope)0)), Encoding.UTF8.GetString(RhinoProtectedData.Unprotect(password, _salt, (DataProtectionScope)0)));
				Trace.WriteLine("After Unprotect");
				item2 = new Uri(Encoding.UTF8.GetString(RhinoProtectedData.Unprotect(proxy, _salt, (DataProtectionScope)0)));
			}
			catch (Exception ex) when (ex is IOException || ex is JsonException || ex is CryptographicException || ex is ArgumentNullException)
			{
			}
			Trace.WriteLine("End _ReadCredentialsFromDisk");
			return Tuple.Create(item, item2);
		}

		private static void _WriteCredentialsToDisk(string storagePath, NetworkCredential credentials, Uri proxyUri)
		{
			Trace.WriteLine("Begin _WriteCredentialsToDisk sotragePath: " + storagePath);
			if (storagePath != null)
			{
				try
				{
					Trace.WriteLine("Before Protect");
					ProxyFile value = new ProxyFile
					{
						Proxy = RhinoProtectedData.Protect(Encoding.UTF8.GetBytes(proxyUri.ToString()), _salt, (DataProtectionScope)0),
						Username = RhinoProtectedData.Protect(Encoding.UTF8.GetBytes(credentials.UserName), _salt, (DataProtectionScope)0),
						Password = RhinoProtectedData.Protect(Encoding.UTF8.GetBytes(credentials.Password), _salt, (DataProtectionScope)0)
					};
					Trace.WriteLine("After Protect");
					string contents = JsonConvert.SerializeObject(value);
					Trace.WriteLine("Before WriteAllText");
					File.WriteAllText(storagePath, contents);
					Trace.WriteLine("After WriteAllText");
				}
				catch (Exception ex) when (ex is IOException || ex is CryptographicException)
				{
				}
				Trace.WriteLine("End _WriteCredentialsToDisk");
			}
		}

		private static bool _CredentialsEqual(NetworkCredential a, NetworkCredential b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			if (a.UserName == b.UserName)
			{
				return a.Password == b.Password;
			}
			return false;
		}

		private static async Task<NetworkCredential> _TryGetProxyCredentialsAsync(CancellationToken cancelToken, Uri proxyUri, bool askUser, NetworkCredential previouslyUsedCredentialsForProxy)
		{
			Trace.WriteLine("Begin _TryGetProxyCredentials");
			if (cancelToken.IsCancellationRequested)
			{
				Trace.WriteLine("Returning in line 180");
				return null;
			}
			lock (_credentialsLock)
			{
				Trace.WriteLine("After _credentialsLock");
				if (_cachedCredentials != null && _cachedCredentialsProxyUri == proxyUri && !_CredentialsEqual(_cachedCredentials, previouslyUsedCredentialsForProxy))
				{
					Trace.WriteLine("Returning in line 187");
					return _cachedCredentials;
				}
				Tuple<NetworkCredential, Uri> tuple = _ReadCredentialsFromDisk(_settingStorageFile);
				if (tuple.Item1 != null && tuple.Item2 == proxyUri && !_CredentialsEqual(tuple.Item1, previouslyUsedCredentialsForProxy))
				{
					_cachedCredentials = tuple.Item1;
					_cachedCredentialsProxyUri = proxyUri;
					Trace.WriteLine("Returning in line 199");
					return _cachedCredentials;
				}
			}
			if (askUser)
			{
				Tuple<NetworkCredential, bool> tuple2 = await _AskForCredentialsAsync(cancelToken, proxyUri).ConfigureAwait(continueOnCapturedContext: false);
				if (tuple2.Item1 != null)
				{
					if (tuple2.Item2)
					{
						_WriteCredentialsToDisk(_settingStorageFile, tuple2.Item1, proxyUri);
					}
					lock (_credentialsLock)
					{
						_cachedCredentials = tuple2.Item1;
						_cachedCredentialsProxyUri = proxyUri;
						Trace.WriteLine("Returning in line 199");
						return _cachedCredentials;
					}
				}
			}
			Trace.WriteLine("End _TryGetProxyCredentials");
			return null;
		}

		public static bool RequestIsProxied(string Uri)
		{
			IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
			if (defaultWebProxy == null)
			{
				return false;
			}
			Uri uri = new Uri(Uri);
			if (defaultWebProxy.GetProxy(uri) == uri)
			{
				return false;
			}
			return true;
		}

		public static HttpClient CreateHttpClient()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			return new HttpClient((HttpMessageHandler)(object)new AuthenticatedProxyHttpClientHandler());
		}

		public static AuthenticatedProxyWebSocket CreateWebSocket()
		{
			return new AuthenticatedProxyWebSocket();
		}

		public static (HttpStatusCode, string) GetSynchronous(HttpClient client, string url)
		{
			try
			{
				return Task.Run(async delegate
				{
					HttpResponseMessage msg = await client.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);
					try
					{
						string item = await msg.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
						HttpStatusCode statusCode = msg.StatusCode;
						return (status: statusCode, response: item);
					}
					finally
					{
						((IDisposable)msg)?.Dispose();
					}
				}).Result;
			}
			catch (Exception ex)
			{
				throw ex.InnerException;
			}
		}

		public static (HttpStatusCode, string) PostSynchronous(HttpClient client, string url, HttpContent content)
		{
			try
			{
				return Task.Run(async delegate
				{
					HttpResponseMessage msg = await client.PostAsync(url, content).ConfigureAwait(continueOnCapturedContext: false);
					try
					{
						string item = await msg.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
						HttpStatusCode statusCode = msg.StatusCode;
						return (status: statusCode, response: item);
					}
					finally
					{
						((IDisposable)msg)?.Dispose();
					}
				}).Result;
			}
			catch (Exception ex)
			{
				throw ex.InnerException;
			}
		}
	}
	internal class ProxyDialog : Dialog<bool>
	{
		public Button OkButton { get; } = new Button
		{
			Text = Localization.LocalizeString("Login", 2),
			Enabled = false
		};

		public Button CancelButton { get; } = new Button
		{
			Text = Localization.LocalizeString("Cancel", 3)
		};

		public TextBox Username { get; } = new TextBox();

		public PasswordBox Password { get; } = new PasswordBox();

		public CheckBox RememberCredentials { get; } = new CheckBox
		{
			Text = Localization.LocalizeString("Remember credentials.", 4),
			ThreeState = false
		};

		public ProxyDialog(Uri proxyUri)
		{
			base.Title = Localization.LocalizeString("Proxy Authentication Required", 5);
			Username.Height = 20;
			Password.Height = Username.Height;
			Username.TextChanged += _TextChanged;
			Password.TextChanged += _TextChanged;
			OkButton.Click += delegate
			{
				Close(result: true);
			};
			CancelButton.Click += delegate
			{
				Close(result: false);
			};
			base.DefaultButton = OkButton;
			base.AbortButton = CancelButton;
			base.Content = new StackLayout
			{
				Orientation = Orientation.Vertical,
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				Spacing = 10,
				Padding = 15,
				Items = 
				{
					(StackLayoutItem)string.Format(Localization.LocalizeString("The proxy {0} requires a username and password.", 6), proxyUri.ToString()),
					(StackLayoutItem)new TableLayout
					{
						Spacing = new Size(10, 10),
						Padding = new Padding(0, 10),
						Rows = 
						{
							new TableRow(new Label
							{
								Text = Localization.LocalizeString("Username", 7),
								TextAlignment = TextAlignment.Right
							}, new TableCell(Username, scaleWidth: true)),
							new TableRow(new Label
							{
								Text = Localization.LocalizeString("Password", 8),
								TextAlignment = TextAlignment.Right
							}, new TableCell(Password, scaleWidth: true)),
							new TableRow(null, RememberCredentials)
						}
					},
					(StackLayoutItem)null,
					(StackLayoutItem)new StackLayout
					{
						Orientation = Orientation.Horizontal,
						Spacing = 10,
						Items = 
						{
							(StackLayoutItem)null,
							(StackLayoutItem)OkButton,
							(StackLayoutItem)CancelButton
						}
					}
				}
			};
		}

		private void _TextChanged(object sender, EventArgs e)
		{
			OkButton.Enabled = !string.IsNullOrEmpty(Password.Text) && !string.IsNullOrEmpty(Username.Text);
		}
	}
	public class ProxyFile
	{
		[JsonProperty("username")]
		public byte[] Username { get; set; }

		[JsonProperty("password")]
		public byte[] Password { get; set; }

		[JsonProperty("proxy")]
		public byte[] Proxy { get; set; }
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
