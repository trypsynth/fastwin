using Microsoft.Win32;
using System.Diagnostics;

while (true) {
	Console.WriteLine("Fastwin - Main Menu.");
	Console.WriteLine("1. Disable Automatic Maintenance.");
	Console.WriteLine("2. Disable Telemetry (Enterprise/Education only).");
	Console.WriteLine("3. Windows Update Configuration.");
	Console.WriteLine("4. Disable Driver Retrieval from Windows Update.");
	Console.WriteLine("5. Make All Wi-Fi connections Metered.");
	Console.WriteLine("6. Disable Folder Discovery.");
	Console.WriteLine("7. Disable Fast Startup");
	Console.WriteLine("8. Make Explorer Show File Extensions.");
	Console.WriteLine("9. Make Explorer Show Hidden Files.");
	Console.WriteLine("10. Make Explorer not Hide Empty Drives.");
	Console.WriteLine("11. Make Explorer open to 'This PC'");
	Console.WriteLine("0.  Exit");
	Console.Write("Select an option: ");
	string? input = Console.ReadLine();
	if (input == "0") break;
	switch (input) {
		case "1":
			SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1);
			break;
		case "2":
			SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
			RunGpUpdate();
			break;
		case "3":
			ConfigureUpdates();
			break;
		case "4":
			SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0);
			break;
		case "5":
			SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost", "cost", 3);
			break;
		case "6":
			SetReg(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType", "NotSpecified", RegistryValueKind.String);
			break;
		case "7":
			SetReg(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0);
			break;
		case "8":
			SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0);
			break;
		case "9":
			SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1);
			break;
		case "10":
			SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia", 0);
			break;
		case "11":
			SetReg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1);
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
			SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1);
			RunGpUpdate();
			break;
		} else if (c == '2') {
			SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0);
			SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 2);
			RunGpUpdate();
			break;
		} else if (c == '3') break;
	}
}

static void SetReg(RegistryKey hive, string path, string name, object value, RegistryValueKind kind = RegistryValueKind.DWord) {
	try {
		using var key = hive.CreateSubKey(path, true);
		key.SetValue(name, value, kind);
		Console.WriteLine("Registry updated successfully.");
	} catch (Exception ex) {
		Console.WriteLine($"Error: {ex.Message}");
	}
}

static void RunGpUpdate() {
	try {
		Process.Start(new ProcessStartInfo("gpupdate", "/force") { CreateNoWindow = true })?.WaitForExit();
		Console.WriteLine("Policy Refreshed.");
	} catch {
		Console.WriteLine("Error: GPUpdate failed.");
	}
}
