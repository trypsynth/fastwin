using Microsoft.Win32;
using System.Diagnostics;

while (true) {
	Console.Clear();
	Console.WriteLine("Fastwin - Main Menu.");
	Console.WriteLine($"1. Disable Automatic Maintenance ({S(IsAutoMaintenanceDisabled())}).");
	Console.WriteLine($"2. Disable Telemetry (Enterprise/Education only) ({S(IsTelemetryDisabled())}).");
	Console.WriteLine("3. Windows Update Configuration.");
	Console.WriteLine($"4. Disable Driver Retrieval from Windows Update ({S(IsDriverRetrievalDisabled())}).");
	Console.WriteLine($"5. Make All Wi-Fi connections Metered ({S(IsWifiMetered())}).");
	Console.WriteLine($"6. Disable Fast Startup ({S(IsFastStartupDisabled())}).");
	Console.WriteLine($"7. Disable Folder Discovery ({S(IsFolderDiscoveryDisabled())}).");
	Console.WriteLine($"8. Make Explorer Show File Extensions ({S(IsShowFileExtensions())}).");
	Console.WriteLine($"9. Make Explorer Show Hidden Files ({S(IsShowHiddenFiles())}).");
	Console.WriteLine($"10. Make Explorer not Hide Empty Drives ({S(IsShowEmptyDrives())}).");
	Console.WriteLine($"11. Make Explorer open to 'This PC' ({S(IsExplorerThisPC())}).");
	Console.WriteLine($"12. Restore Classic Right-Click Context Menu (Win 11) ({S(IsClassicContextMenu())}).");
	Console.WriteLine($"13. Disable Bing Search in Start Menu ({S(IsBingSearchDisabled())}).");
	Console.WriteLine("0. Exit.");
	Console.Write("\nSelect an option: ");
	string? input = Console.ReadLine();
	if (input == "0") break;
	Console.Clear();
	switch (input) {
		case "1":
			if (IsAutoMaintenanceDisabled())
				Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 0));
			else
				Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1));
			break;
		case "2":
			if (IsTelemetryDisabled())
				Report(DeleteRegValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry"));
			else
				Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0));
			RunGpUpdate();
			break;
		case "3":
			ConfigureUpdates();
			break;
		case "4":
			if (IsDriverRetrievalDisabled())
				Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 1));
			else
				Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0));
			break;
		case "5":
			if (IsWifiMetered())
				Report(DeleteRegValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost", "cost"));
			else
				Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost", "cost", 3));
			break;
		case "6":
			if (IsFastStartupDisabled())
				Report(SetReg(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 1));
			else
				Report(SetReg(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0));
			break;
		case "7":
			if (IsFolderDiscoveryDisabled())
				Report(DeleteRegValue(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType"));
			else
				Report(SetReg(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType", "NotSpecified", RegistryValueKind.String));
			break;
		case "8":
			if (IsShowFileExtensions())
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1));
			else
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0));
			break;
		case "9":
			if (IsShowHiddenFiles())
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 2));
			else
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1));
			break;
		case "10":
			if (IsShowEmptyDrives())
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia", 1));
			else
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia", 0));
			break;
		case "11":
			if (IsExplorerThisPC())
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 2));
			else
				Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1));
			break;
		case "12":
			if (IsClassicContextMenu())
				Report(DeleteRegKey(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"));
			else
				Report(SetReg(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String));
			Console.WriteLine("Restart Explorer for the change to take effect.");
			break;
		case "13":
			if (IsBingSearchDisabled()) {
				Report(
					SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 1),
					SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 1)
				);
			} else {
				Report(
					SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0),
					SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0)
				);
			}
			break;
		default:
			Console.WriteLine("[!] Invalid selection.");
			break;
	}
}

static bool IsAutoMaintenanceDisabled() =>
	GetRegDWord(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled") == 1;

static bool IsTelemetryDisabled() =>
	GetRegDWord(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry") == 0;

static bool IsDriverRetrievalDisabled() =>
	GetRegDWord(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig") == 0;

static bool IsWifiMetered() =>
	GetRegDWord(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost", "cost") == 3;

static bool IsFastStartupDisabled() =>
	GetRegDWord(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled") == 0;

static bool IsFolderDiscoveryDisabled() =>
	GetRegString(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType") == "NotSpecified";

static bool IsShowFileExtensions() =>
	GetRegDWord(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt") == 0;

static bool IsShowHiddenFiles() =>
	GetRegDWord(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden") == 1;

static bool IsShowEmptyDrives() =>
	GetRegDWord(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia") == 0;

static bool IsExplorerThisPC() =>
	GetRegDWord(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo") == 1;

static bool IsClassicContextMenu() =>
	RegKeyExists(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32");

static bool IsBingSearchDisabled() =>
	GetRegDWord(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled") == 0;

static string S(bool active) => active ? "ON" : "OFF";

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

static void ConfigureUpdates() {
	while (true) {
		Console.WriteLine("Windows Update Configuration.");
		Console.WriteLine("1. Disable Updates Completely.");
		Console.WriteLine("2. Notify Only.");
		Console.WriteLine("3. Back.");
		Console.Write("Select an option: ");
		char c = Console.ReadKey(true).KeyChar;
		if (c == '1') {
			Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1));
			RunGpUpdate();
			break;
		} else if (c == '2') {
			Report(
				SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0),
				SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 2)
			);
			RunGpUpdate();
			break;
		} else if (c == '3') break;
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

static void RunGpUpdate() {
	try {
		Process.Start(new ProcessStartInfo("gpupdate", "/force") { CreateNoWindow = true })?.WaitForExit();
		Console.WriteLine("Policy refreshed.");
	} catch {
		Console.WriteLine("Error: gpupdate failed.");
	}
}
