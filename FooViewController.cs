using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Views;
using System.Runtime.InteropServices;
using MauiB2C.MSALClient;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;

namespace popup_repro;

public class FooViewController : UIViewController
{
    public override void LoadView()
    {
        base.LoadView();
        View!.BackgroundColor = UIColor.SystemBackground;

        var btn = new UIButton()
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        var config = UIButtonConfiguration.GrayButtonConfiguration;
        config.BaseForegroundColor = UIColor.SystemBlue;
        config.Title = "Login";
        btn.Configuration = config;
        btn.TouchUpInside += async (sender, _) =>
        {
            var tSender = (UIButton)sender!;

            tSender.Enabled = false;
            await DoLogin();
            tSender.Enabled = true;
        };

        View.Add(btn);

        NSLayoutConstraint.ActivateConstraints(new[]
        {
            btn.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
            btn.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
        });
    }

    private async Task<bool> DoLogin()
    {
        var parent = this;
        var loginSvc = new LoginService();
        var success = await loginSvc.SignInAsync(parent).ConfigureAwait(false);

        var notifyService = new NotifyService();
        var msg = success ? "Login Successful" : "There was an issue logging in. Check your credentials and try again.";
        await notifyService.Toast(msg).ConfigureAwait(false);

        return success;
    }
}

internal class LoginService
{
    public async Task<bool> SignInAsync(UIViewController parent)
    {
        var result = await PCAWrapperB2C.Instance.AcquireTokenInteractiveAsync(parent).ConfigureAwait(false);
        return result;
    }
}

internal class NotifyService
{
    public async Task Toast(string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Task.Delay(0);
            SemanticScreenReader.Announce(message);

            try
            {
                // NOTE(jpr): copied from https://github.com/CommunityToolkit/Maui/blob/721b67a3360c594572a4c0ca778813951e95dbe9/src/CommunityToolkit.Maui/Alerts/Toast/Toast.macios.cs#L33
                var cornerRadius = new CGRect(4, 4, 4, 4);
                var padding = new[] { cornerRadius.X, cornerRadius.Y, cornerRadius.Width, cornerRadius.Height }.Max();
                var alert = new PlatformToast(
                    message,
                    AlertDefaults.BackgroundColor.ToPlatform(),
                    cornerRadius,
                    AlertDefaults.TextColor.ToPlatform(),
                    UIFont.SystemFontOfSize((NFloat)AlertDefaults.FontSize),
                    AlertDefaults.CharacterSpacing,
                    padding)
                {
                    Duration = TimeSpan.FromSeconds(2),
                };

                alert.Show();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while showing Toast");
                Console.WriteLine(e.Message);
            }
        });
    }
}
