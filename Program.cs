using Microsoft.Win32;
using System.Diagnostics;

while (true) {
	Console.WriteLine("Fastwin - Main Menu.");
	Console.WriteLine("1. Disable Automatic Maintenance.");
	Console.WriteLine("2. Disable Telemetry (Enterprise/Education only).");
	Console.WriteLine("3. Windows Update Configuration.");
	Console.WriteLine("4. Disable Driver Retrieval from Windows Update.");
	Console.WriteLine("5. Make All Wi-Fi connections Metered.");
	Console.WriteLine("6. Disable Fast Startup.");
	Console.WriteLine("7. Disable Folder Discovery.");
	Console.WriteLine("8. Make Explorer Show File Extensions.");
	Console.WriteLine("9. Make Explorer Show Hidden Files.");
	Console.WriteLine("10. Make Explorer not Hide Empty Drives.");
	Console.WriteLine("11. Make Explorer open to 'This PC'.");
	Console.WriteLine("12. Restore Classic Right-Click Context Menu (Win 11).");
	Console.WriteLine("13. Disable Bing Search in Start Menu.");
	Console.WriteLine("0. Exit.");
	Console.Write("Select an option: ");
	string? input = Console.ReadLine();
	if (input == "0") break;
	switch (input) {
		case "1":
			Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1));
			break;
		case "2":
			Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0));
			RunGpUpdate();
			break;
		case "3":
			ConfigureUpdates();
			break;
		case "4":
			Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0));
			break;
		case "5":
			Report(SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost", "cost", 3));
			break;
		case "6":
			Report(SetReg(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0));
			break;
		case "7":
			Report(SetReg(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType", "NotSpecified", RegistryValueKind.String));
			break;
		case "8":
			Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0));
			break;
		case "9":
			Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1));
			break;
		case "10":
			Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia", 0));
			break;
		case "11":
			Report(SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1));
			break;
		case "12":
			// Registering an empty InprocServer32 entry overrides the Win11 context menu host with nothing, which causes Windows to fall back to the classic IContextMenu shell extension path.
			Report(SetReg(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String));
			Console.WriteLine("Restart Explorer for the change to take effect.");
			break;
		case "13":
			Report(
				SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0),
				SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0)
			);
			break;
		default:
			Console.WriteLine("[!] Invalid selection.");
			break;
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
