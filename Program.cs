using Microsoft.Win32;
using System.Diagnostics;

Category[] categories = [
	new("Privacy and Ads", [
		Toggle("Disable Telemetry (Enterprise/Education only)", IsTelemetryDisabled,
			() => { Report(SetReg(Registry.LocalMachine, DataCollectionPolicy, "AllowTelemetry", 0)); RunGpUpdate(); },
			() => { Report(DeleteRegValue(Registry.LocalMachine, DataCollectionPolicy, "AllowTelemetry")); RunGpUpdate(); }),
		Toggle("Disable Advertising ID", IsAdvertisingIdDisabled,
			() => Report(SetReg(Registry.CurrentUser, AdvertisingInfoKey, "Enabled", 0)),
			() => Report(SetReg(Registry.CurrentUser, AdvertisingInfoKey, "Enabled", 1))),
		Toggle("Disable Suggested Apps, Tips and Ads", IsSuggestionsDisabled,
			() => Report(Array.ConvertAll(SuggestionValues, name => SetReg(Registry.CurrentUser, ContentDeliveryManagerKey, name, 0))),
			() => Report(Array.ConvertAll(SuggestionValues, name => SetReg(Registry.CurrentUser, ContentDeliveryManagerKey, name, 1)))),
		Toggle("Disable 'Finish Setting Up Your Device' Prompt", IsSetupNagDisabled,
			() => Report(SetReg(Registry.CurrentUser, UserProfileEngagementKey, "ScoobeSystemSettingEnabled", 0)),
			() => Report(SetReg(Registry.CurrentUser, UserProfileEngagementKey, "ScoobeSystemSettingEnabled", 1))),
		Toggle("Disable Bing Search in Start Menu", IsBingSearchDisabled,
			() => {
				Report(
					SetReg(Registry.CurrentUser, SearchKey, "BingSearchEnabled", 0),
					SetReg(Registry.CurrentUser, SearchKey, "CortanaConsent", 0),
					SetReg(Registry.CurrentUser, ExplorerPolicy, "DisableSearchBoxSuggestions", 1)
				);
				RestartExplorerNote();
			},
			() => {
				Report(
					SetReg(Registry.CurrentUser, SearchKey, "BingSearchEnabled", 1),
					SetReg(Registry.CurrentUser, SearchKey, "CortanaConsent", 1),
					DeleteRegValue(Registry.CurrentUser, ExplorerPolicy, "DisableSearchBoxSuggestions")
				);
				RestartExplorerNote();
			}),
	]),
	new("Windows Update", [
		new("Disable Updates Completely", () => {
			Report(SetReg(Registry.LocalMachine, WindowsUpdatePolicy, "NoAutoUpdate", 1));
			RunGpUpdate();
		}),
		new("Notify Only", () => {
			Report(
				SetReg(Registry.LocalMachine, WindowsUpdatePolicy, "NoAutoUpdate", 0),
				SetReg(Registry.LocalMachine, WindowsUpdatePolicy, "AUOptions", 2)
			);
			RunGpUpdate();
		}),
		Toggle("Disable Driver Retrieval from Windows Update", IsDriverRetrievalDisabled,
			() => Report(SetReg(Registry.LocalMachine, DriverSearchingKey, "SearchOrderConfig", 0)),
			() => Report(SetReg(Registry.LocalMachine, DriverSearchingKey, "SearchOrderConfig", 1))),
		Toggle("Disable Sharing Updates with Other PCs", IsPeerUpdateSharingDisabled,
			() => { Report(SetReg(Registry.LocalMachine, DeliveryOptimizationPolicy, "DODownloadMode", 0)); RunGpUpdate(); },
			() => { Report(DeleteRegValue(Registry.LocalMachine, DeliveryOptimizationPolicy, "DODownloadMode")); RunGpUpdate(); }),
		Toggle("Don't Auto-Restart While Signed In", IsAutoRebootDisabled,
			() => { Report(SetReg(Registry.LocalMachine, WindowsUpdatePolicy, "NoAutoRebootWithLoggedOnUsers", 1)); RunGpUpdate(); },
			() => { Report(DeleteRegValue(Registry.LocalMachine, WindowsUpdatePolicy, "NoAutoRebootWithLoggedOnUsers")); RunGpUpdate(); }),
	]),
	new("File Explorer", [
		Toggle("Show File Extensions", IsShowFileExtensions,
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "HideFileExt", 0)),
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "HideFileExt", 1))),
		Toggle("Show Hidden Files", IsShowHiddenFiles,
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "Hidden", 1)),
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "Hidden", 2))),
		Toggle("Don't Hide Empty Drives", IsShowEmptyDrives,
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "HideDrivesWithNoMedia", 0)),
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "HideDrivesWithNoMedia", 1))),
		Toggle("Open to 'This PC'", IsExplorerThisPC,
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "LaunchTo", 1)),
			() => Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "LaunchTo", 2))),
		Toggle("Restore Classic Right-Click Context Menu (Win 11)", IsClassicContextMenu,
			() => { Report(SetReg(Registry.CurrentUser, ClassicContextMenuKey + @"\InprocServer32", "", "", RegistryValueKind.String)); RestartExplorerNote(); },
			() => { Report(DeleteRegKey(Registry.CurrentUser, ClassicContextMenuKey)); RestartExplorerNote(); }),
		Toggle("Enable Checkbox Selection", IsCheckboxSelectionEnabled,
			() => { Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "AutoCheckSelect", 1)); RestartExplorerNote(); },
			() => { Report(SetReg(Registry.CurrentUser, ExplorerAdvancedKey, "AutoCheckSelect", 0)); RestartExplorerNote(); }),
		Toggle("Disable ' - Shortcut' Suffix on New Shortcuts", IsShortcutSuffixDisabled,
			() => { Report(SetReg(Registry.CurrentUser, ExplorerKey, "link", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary)); RestartExplorerNote(); },
			() => { Report(SetReg(Registry.CurrentUser, ExplorerKey, "link", new byte[] { 0x1e, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary)); RestartExplorerNote(); }),
		Toggle("Hide Recent Files and Frequent Folders", IsRecentItemsHidden,
			() => {
				Report(SetReg(Registry.CurrentUser, ExplorerKey, "ShowRecent", 0), SetReg(Registry.CurrentUser, ExplorerKey, "ShowFrequent", 0));
				RestartExplorerNote();
			},
			() => {
				Report(SetReg(Registry.CurrentUser, ExplorerKey, "ShowRecent", 1), SetReg(Registry.CurrentUser, ExplorerKey, "ShowFrequent", 1));
				RestartExplorerNote();
			}),
		Toggle("Disable Folder Discovery", IsFolderDiscoveryDisabled,
			() => Report(SetReg(Registry.CurrentUser, AllFoldersShellKey, "FolderType", "NotSpecified", RegistryValueKind.String)),
			() => Report(DeleteRegValue(Registry.CurrentUser, AllFoldersShellKey, "FolderType"))),
	]),
	new("System", [
		Toggle("Disable Automatic Maintenance", IsAutoMaintenanceDisabled,
			() => Report(SetReg(Registry.LocalMachine, MaintenanceKey, "MaintenanceDisabled", 1)),
			() => Report(SetReg(Registry.LocalMachine, MaintenanceKey, "MaintenanceDisabled", 0))),
		Toggle("Disable Fast Startup", IsFastStartupDisabled,
			() => Report(SetReg(Registry.LocalMachine, PowerKey, "HiberbootEnabled", 0)),
			() => Report(SetReg(Registry.LocalMachine, PowerKey, "HiberbootEnabled", 1))),
		Toggle("Make All Wi-Fi Connections Metered", IsWifiMetered,
			() => Report(SetReg(Registry.LocalMachine, NetCostPolicy, "cost", 3)),
			() => Report(DeleteRegValue(Registry.LocalMachine, NetCostPolicy, "cost"))),
		Toggle("Disable Search Indexer", IsSearchIndexerDisabled,
			() => { Report(SetReg(Registry.LocalMachine, WSearchServiceKey, "Start", 4)); RunHidden("sc", "stop WSearch"); },
			() => { Report(SetReg(Registry.LocalMachine, WSearchServiceKey, "Start", 2)); RunHidden("sc", "start WSearch"); }),
		Toggle("Enable Long Paths (over 260 characters)", IsLongPathsEnabled,
			() => Report(SetReg(Registry.LocalMachine, FileSystemKey, "LongPathsEnabled", 1)),
			() => Report(SetReg(Registry.LocalMachine, FileSystemKey, "LongPathsEnabled", 0))),
	]),
	new("Apps", [
		Toggle("Disable OneDrive", IsOneDriveDisabled,
			() => { Report(SetReg(Registry.LocalMachine, OneDrivePolicy, "DisableFileSyncNGSC", 1)); RunGpUpdate(); SignOutNote(); },
			() => { Report(DeleteRegValue(Registry.LocalMachine, OneDrivePolicy, "DisableFileSyncNGSC")); RunGpUpdate(); SignOutNote(); }),
		Toggle("Stop Edge Running in the Background", IsEdgeBackgroundDisabled,
			() => Report(SetReg(Registry.LocalMachine, EdgePolicy, "StartupBoostEnabled", 0), SetReg(Registry.LocalMachine, EdgePolicy, "BackgroundModeEnabled", 0)),
			() => Report(DeleteRegValue(Registry.LocalMachine, EdgePolicy, "StartupBoostEnabled"), DeleteRegValue(Registry.LocalMachine, EdgePolicy, "BackgroundModeEnabled"))),
	]),
];

while (true) {
	Console.Clear();
	Console.WriteLine("Fastwin - Main Menu.");
	for (int i = 0; i < categories.Length; i++)
		Console.WriteLine($"{i + 1}. {categories[i].Name}.");
	Console.WriteLine("0. Exit.");
	Console.Write("\nSelect an option: ");
	int choice = ReadChoice(categories.Length);
	if (choice == 0) break;
	ShowCategory(categories[choice - 1]);
}

static void ShowCategory(Category category) {
	while (true) {
		Console.Clear();
		Console.WriteLine($"Fastwin - {category.Name}.");
		for (int i = 0; i < category.Items.Length; i++) {
			MenuItem item = category.Items[i];
			string state = item.State == null ? "" : $" ({S(item.State())})";
			Console.WriteLine($"{i + 1}. {item.Label}{state}.");
		}
		Console.WriteLine("0. Back.");
		Console.Write("\nSelect an option: ");
		int choice = ReadChoice(category.Items.Length);
		if (choice == 0) return;
		Console.Clear();
		category.Items[choice - 1].Run();
		Console.Write("Press any key to continue...");
		Console.ReadKey(true);
	}
}

// Waits for a single digit key from 0 to max, ignoring anything else. Menus must stay at 9 items or fewer.
static int ReadChoice(int max) {
	while (true) {
		char c = Console.ReadKey(true).KeyChar;
		if (c >= '0' && c <= '0' + max) return c - '0';
	}
}

static MenuItem Toggle(string label, Func<bool> isOn, Action turnOn, Action turnOff) =>
	new(label, () => { if (isOn()) turnOff(); else turnOn(); }, isOn);

static bool IsAutoMaintenanceDisabled() =>
	GetRegDWord(Registry.LocalMachine, MaintenanceKey, "MaintenanceDisabled") == 1;

static bool IsTelemetryDisabled() =>
	GetRegDWord(Registry.LocalMachine, DataCollectionPolicy, "AllowTelemetry") == 0;

static bool IsDriverRetrievalDisabled() =>
	GetRegDWord(Registry.LocalMachine, DriverSearchingKey, "SearchOrderConfig") == 0;

static bool IsWifiMetered() =>
	GetRegDWord(Registry.LocalMachine, NetCostPolicy, "cost") == 3;

static bool IsFastStartupDisabled() =>
	GetRegDWord(Registry.LocalMachine, PowerKey, "HiberbootEnabled") == 0;

static bool IsFolderDiscoveryDisabled() =>
	GetRegString(Registry.CurrentUser, AllFoldersShellKey, "FolderType") == "NotSpecified";

static bool IsShowFileExtensions() =>
	GetRegDWord(Registry.CurrentUser, ExplorerAdvancedKey, "HideFileExt") == 0;

static bool IsShowHiddenFiles() =>
	GetRegDWord(Registry.CurrentUser, ExplorerAdvancedKey, "Hidden") == 1;

static bool IsShowEmptyDrives() =>
	GetRegDWord(Registry.CurrentUser, ExplorerAdvancedKey, "HideDrivesWithNoMedia") == 0;

static bool IsExplorerThisPC() =>
	GetRegDWord(Registry.CurrentUser, ExplorerAdvancedKey, "LaunchTo") == 1;

static bool IsClassicContextMenu() =>
	RegKeyExists(Registry.CurrentUser, ClassicContextMenuKey + @"\InprocServer32");

static bool IsBingSearchDisabled() =>
	GetRegDWord(Registry.CurrentUser, SearchKey, "BingSearchEnabled") == 0 &&
	GetRegDWord(Registry.CurrentUser, ExplorerPolicy, "DisableSearchBoxSuggestions") == 1;

static bool IsShortcutSuffixDisabled() {
	byte[]? link = GetRegBinary(Registry.CurrentUser, ExplorerKey, "link");
	return link != null && Array.TrueForAll(link, b => b == 0);
}

static bool IsCheckboxSelectionEnabled() =>
	GetRegDWord(Registry.CurrentUser, ExplorerAdvancedKey, "AutoCheckSelect") == 1;

static bool IsOneDriveDisabled() =>
	GetRegDWord(Registry.LocalMachine, OneDrivePolicy, "DisableFileSyncNGSC") == 1;

static bool IsSuggestionsDisabled() =>
	Array.TrueForAll(SuggestionValues, name => GetRegDWord(Registry.CurrentUser, ContentDeliveryManagerKey, name) == 0);

static bool IsSearchIndexerDisabled() =>
	GetRegDWord(Registry.LocalMachine, WSearchServiceKey, "Start") == 4;

static bool IsEdgeBackgroundDisabled() =>
	GetRegDWord(Registry.LocalMachine, EdgePolicy, "StartupBoostEnabled") == 0 &&
	GetRegDWord(Registry.LocalMachine, EdgePolicy, "BackgroundModeEnabled") == 0;

static bool IsAdvertisingIdDisabled() =>
	GetRegDWord(Registry.CurrentUser, AdvertisingInfoKey, "Enabled") == 0;

static bool IsSetupNagDisabled() =>
	GetRegDWord(Registry.CurrentUser, UserProfileEngagementKey, "ScoobeSystemSettingEnabled") == 0;

static bool IsLongPathsEnabled() =>
	GetRegDWord(Registry.LocalMachine, FileSystemKey, "LongPathsEnabled") == 1;

static bool IsRecentItemsHidden() =>
	GetRegDWord(Registry.CurrentUser, ExplorerKey, "ShowRecent") == 0 &&
	GetRegDWord(Registry.CurrentUser, ExplorerKey, "ShowFrequent") == 0;

static bool IsPeerUpdateSharingDisabled() =>
	GetRegDWord(Registry.LocalMachine, DeliveryOptimizationPolicy, "DODownloadMode") == 0;

static bool IsAutoRebootDisabled() =>
	GetRegDWord(Registry.LocalMachine, WindowsUpdatePolicy, "NoAutoRebootWithLoggedOnUsers") == 1;

static string S(bool active) => active ? "ON" : "OFF";

static void RestartExplorerNote() => Console.WriteLine("Restart Explorer for the change to take effect.");

static void SignOutNote() => Console.WriteLine("Sign out and back in for the change to take full effect.");

static object? GetReg(RegistryKey hive, string path, string name) {
	try {
		using var key = hive.OpenSubKey(path);
		return key?.GetValue(name);
	} catch {
		return null;
	}
}

static int GetRegDWord(RegistryKey hive, string path, string name) =>
	GetReg(hive, path, name) is int i ? i : -1;

static string? GetRegString(RegistryKey hive, string path, string name) =>
	GetReg(hive, path, name) as string;

static byte[]? GetRegBinary(RegistryKey hive, string path, string name) =>
	GetReg(hive, path, name) as byte[];

static bool RegKeyExists(RegistryKey hive, string path) {
	try {
		using var key = hive.OpenSubKey(path);
		return key != null;
	} catch {
		return false;
	}
}

static string? DeleteRegValue(RegistryKey hive, string path, string name) {
	try {
		using var key = hive.OpenSubKey(path, true);
		key?.DeleteValue(name, false);
		return null;
	} catch (Exception ex) {
		return ex.Message;
	}
}

static string? DeleteRegKey(RegistryKey hive, string path) {
	try {
		hive.DeleteSubKeyTree(path, false);
		return null;
	} catch (Exception ex) {
		return ex.Message;
	}
}

static void Report(params string?[] results) {
	var errors = Array.FindAll(results, r => r != null);
	if (errors.Length == 0)
		Console.WriteLine("Done.");
	else
		foreach (var e in errors) Console.WriteLine($"Error: {e}");
}

static string? SetReg(RegistryKey hive, string path, string name, object value, RegistryValueKind kind = RegistryValueKind.DWord) {
	try {
		using var key = hive.CreateSubKey(path, true);
		key.SetValue(name, value, kind);
		return null;
	} catch (Exception ex) {
		return ex.Message;
	}
}

static void RunHidden(string file, string args) {
	try {
		Process.Start(new ProcessStartInfo(file, args) { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true })?.WaitForExit();
	} catch {
		Console.WriteLine($"Error: failed to run {file}.");
	}
}

static void RunGpUpdate() {
	try {
		Process.Start(new ProcessStartInfo("gpupdate", "/force") { CreateNoWindow = true })?.WaitForExit();
		Console.WriteLine("Policy refreshed.");
	} catch {
		Console.WriteLine("Error: gpupdate failed.");
	}
}

record MenuItem(string Label, Action Run, Func<bool>? State = null);

record Category(string Name, MenuItem[] Items);

static partial class Program {
	const string ExplorerKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer";
	const string ExplorerAdvancedKey = ExplorerKey + @"\Advanced";
	const string SearchKey = @"Software\Microsoft\Windows\CurrentVersion\Search";
	const string ContentDeliveryManagerKey = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
	const string AdvertisingInfoKey = @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
	const string UserProfileEngagementKey = @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement";
	const string ClassicContextMenuKey = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";
	const string AllFoldersShellKey = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell";
	const string ExplorerPolicy = @"Software\Policies\Microsoft\Windows\Explorer";
	const string MaintenanceKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance";
	const string DriverSearchingKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching";
	const string PowerKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";
	const string FileSystemKey = @"SYSTEM\CurrentControlSet\Control\FileSystem";
	const string WSearchServiceKey = @"SYSTEM\CurrentControlSet\Services\WSearch";
	const string DataCollectionPolicy = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";
	const string WindowsUpdatePolicy = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
	const string DeliveryOptimizationPolicy = @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization";
	const string NetCostPolicy = @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost";
	const string OneDrivePolicy = @"SOFTWARE\Policies\Microsoft\Windows\OneDrive";
	const string EdgePolicy = @"SOFTWARE\Policies\Microsoft\Edge";

	static readonly string[] SuggestionValues = [
		"SilentInstalledAppsEnabled",       // Auto-installed "suggested" apps.
		"SystemPaneSuggestionsEnabled",     // Suggestions in Start.
		"SubscribedContent-338388Enabled",  // Start menu suggestions.
		"SubscribedContent-338389Enabled",  // Tips and tricks notifications.
		"SubscribedContent-310093Enabled",  // Post-update welcome experience.
		"SubscribedContent-353694Enabled",  // Suggested content in Settings.
		"SubscribedContent-353696Enabled",  // Suggested content in Settings.
	];
}
