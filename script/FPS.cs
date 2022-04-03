using Godot;
using System.Threading.Tasks;
using ExtensionMethods;

public class FPS : Godot.Label
{
    [Signal]
    delegate void WriteFPS(float fps);
    [Signal]
    delegate void HideFPS();

    public override void _Ready()
    {
        SetCheckingFPS(true);
    }

    public bool checking = false;
    public void SetCheckingFPS(bool shouldCheck = true)
    {
        Visible = shouldCheck;
        if (shouldCheck && !checking) {
            checking = true;
            CheckFPS();
        } else if (!shouldCheck && checking) {
            checking = false;
            EmitSignal("HideFPS");
        }
    }

    public async void CheckFPS()
    {
        while (checking) {
            this.EmitSignal("WriteFPS", Godot.Engine.GetFramesPerSecond());
            Text = "FPS: " + Godot.Engine.GetFramesPerSecond().ToString();
            await this.TimerAsync(1.0F);
        }
    }
}
