using Microsoft.Win32;
using System.Diagnostics;

Console.WriteLine("Welcome to Fastwin.");
if (AskYesNo("Disable Automatic Maintenance?")) {
	SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1);
}
if (AskYesNo("Disable Telemetry (Enterprise/Education Only)?")) {
	SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
	RunGpUpdate();
}
bool updateHandled = false;
while (!updateHandled) {
	Console.WriteLine("Windows Update Configuration:");
	Console.WriteLine("1. Disable Automatic Updates completely.");
	Console.WriteLine("2. Notify Only.");
	Console.WriteLine("3. Skip this section.");
	char choice = Console.ReadKey(true).KeyChar;
	Console.WriteLine(choice);
	if (choice == '1') {
		SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1);
		RunGpUpdate();
		updateHandled = true;
	} else if (choice == '2') {
		SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0);
		SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 2);
		RunGpUpdate();
		updateHandled = true;
	} else if (choice == '3') {
		updateHandled = true;
	}
}
if (AskYesNo("Disable Driver Retrieval from Windows Update on device plug-in?")) {
	SetReg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0);
}
if (AskYesNo("Force all Wi-Fi connections to be 'Metered' by default?")) {
	SetReg(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Wireless\NetCost", "cost", 3);
}
if (AskYesNo("Disable Folder Discovery for faster folder loading?")) {
	SetReg(Registry.CurrentUser, @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType", "NotSpecified", RegistryValueKind.String);
}
Console.WriteLine("Optimization Complete. A reboot is recommended for all changes to take effect.");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static bool AskYesNo(string question) {
	while (true) {
		Console.Write($"\n{question} (y/n): ");
		char key = char.ToLower(Console.ReadKey().KeyChar);
		Console.WriteLine();
		if (key == 'y') return true;
		if (key == 'n') return false;
	}
}

static void SetReg(RegistryKey hive, string path, string name, object value, RegistryValueKind kind = RegistryValueKind.DWord) {
	try {
		using var key = hive.CreateSubKey(path, true);
		key.SetValue(name, value, kind);
		Console.WriteLine($"Success: {name} updated.");
	} catch (UnauthorizedAccessException) {
		Console.WriteLine($"Error: Access Denied. Make sure you're running Fastwin as administrator!");
	} catch (Exception ex) {
		Console.WriteLine($"Error: {ex.Message}");
	}
}

static void RunGpUpdate() {
	try {
		Console.WriteLine("Updating group policy...");
		var psi = new ProcessStartInfo("gpupdate", "/force") {
			CreateNoWindow = true,
			UseShellExecute = true
		};
		Process.Start(psi)?.WaitForExit();
	} catch {
		Console.WriteLine("[!] Could not refresh Group Policy.");
	}
}
