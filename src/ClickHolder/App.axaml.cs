using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ClickHolder;

public class App : Application
{
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			// Prevent automatic shutdown
			desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
		}

		base.OnFrameworkInitializationCompleted();
	}

	private void OnExitButtonClick(object? sender, EventArgs e)
	{
		Environment.Exit(0);
	}
}