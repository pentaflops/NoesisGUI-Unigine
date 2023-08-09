namespace UnigineApp;

public class NoesisGUI
{
	public Unigine.ObjectGui objectGui;

	private Noesis.View view;
	private Noesis.RenderDevice device;
	private Noesis.FrameworkElement xaml;

	private Unigine.WidgetSprite hud;
	private Unigine.Texture renderTexture;
	private Unigine.Gui gui;

	private bool mousePressed = false;
	private double ticks;

	private int renderTextureWidth;
	private int renderTextureHeight;

	public NoesisGUI()
	{
		renderTextureWidth = 500;
		renderTextureHeight = 500;
	}

	public void Initialize()
	{
		#region General Noesis init

		Noesis.GUI.Init();

		Noesis.GUI.SetXamlProvider(new NoesisApp.LocalXamlProvider("."));
		Noesis.GUI.SetTextureProvider(new NoesisApp.LocalTextureProvider("."));
		Noesis.GUI.SetFontProvider(new NoesisApp.LocalFontProvider("."));

		NoesisApp.Application.SetThemeProviders();
		Noesis.GUI.LoadApplicationResources("Theme/NoesisTheme.DarkBlue.xaml"); // https://github.com/Noesis/Managed/tree/master/Src/Noesis/Extensions/Theme

		#endregion

		#region Create Noesis view

		xaml = (Noesis.FrameworkElement) Noesis.GUI.ParseXaml(@"
                <Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                    <Grid.Background>
                        <LinearGradientBrush StartPoint=""0,0"" EndPoint=""0,1"">
                            <GradientStop Offset=""0"" Color=""#80123F61""/>
                            <GradientStop Offset=""0.6"" Color=""#800E4B79""/>
                            <GradientStop Offset=""0.7"" Color=""#80106097""/>
                        </LinearGradientBrush>
                    </Grid.Background>
                    <Viewbox>
                        <StackPanel Margin=""50"">
                            <Button Content=""Hello World!"" Margin=""0,30,0,0""/>
                            <Rectangle Height=""5"" Margin=""-10,20,-10,0"">
                                <Rectangle.Fill>
                                    <RadialGradientBrush>
                                        <GradientStop Offset=""0"" Color=""#40000000""/>
                                        <GradientStop Offset=""1"" Color=""#00000000""/>
                                    </RadialGradientBrush>
                                </Rectangle.Fill>
                            </Rectangle>
                        </StackPanel>
                    </Viewbox>
                </Grid>");

		view = Noesis.GUI.CreateView(xaml);
		view.SetFlags(Noesis.RenderFlags.PPAA);
		view.SetTessellationMaxPixelError(Noesis.TessellationMaxPixelError.HighQuality);
		view.SetSize(renderTextureWidth, renderTextureHeight);

		#endregion

		// Init view render device
		device = new Noesis.RenderDeviceD3D11(Unigine.Render.GetD3D11Context());
		view.Renderer.Init(device);

		// If necessary, you can render UI in ObjectGui
		//gui = objectGui.GetGui();
		gui = Unigine.Gui.GetCurrent();
		gui.TransparentEnabled = true;

		hud = new Unigine.WidgetSprite(gui);
		hud.SetPosition(0, 0);
		hud.Width = renderTextureWidth;
		hud.Height = renderTextureHeight;
		hud.SetLayerBlendFunc(0, Unigine.Gui.BLEND_ONE, Unigine.Gui.BLEND_ONE_MINUS_SRC_ALPHA);

		gui.AddChild(hud, Unigine.Gui.ALIGN_OVERLAP);

		renderTexture = new Unigine.Texture();
		renderTexture.Create2D(renderTextureWidth, renderTextureHeight, Unigine.Texture.FORMAT_RGBA8, Unigine.Texture.FORMAT_USAGE_RENDER);

		hud.SetRender(renderTexture);

		Unigine.Render.AddCallback(Unigine.Render.CALLBACK_INDEX.END, Render);
		
		// If you want to render the UI behind the GUI, you can bind to the window renderer
		//Unigine.WindowManager.MainWindow.AddCallback(Unigine.EngineWindow.CALLBACK_INDEX.FUNC_BEGIN_RENDER_GUI, Render);
	}

	private void Render()
	{
		if (view == null)
			return;

		var tmpRenderTarget = Unigine.Render.GetTemporaryRenderTarget();

		tmpRenderTarget.BindColorTexture(0, renderTexture);
		tmpRenderTarget.Enable();

		view.Renderer.UpdateRenderTree();
		view.Renderer.RenderOffscreen();

		view.Renderer.Render(false, true);

		tmpRenderTarget.Disable();
		tmpRenderTarget.UnbindAll();

		Unigine.Render.ReleaseTemporaryRenderTarget(tmpRenderTarget);

	}

	public void Update()
	{
		if (view == null)
			return;

		ticks += Unigine.Game.IFps;
		view.Update(ticks);

		// Depending on the render target, use different ways to update cursor positions
		var windowViewport = Unigine.WindowManager.MainWindow;
		int mouseX = Unigine.Input.MousePosition.x - windowViewport.ClientPosition.x;
		int mouseY = Unigine.Input.MousePosition.y - windowViewport.ClientPosition.y;

		view.MouseMove(mouseX, mouseY);

		if (!mousePressed && Unigine.Input.IsMouseButtonDown(Unigine.Input.MOUSE_BUTTON.LEFT))
		{
			view.MouseButtonDown(mouseX, mouseY, Noesis.MouseButton.Left);
			mousePressed = true;
		}
		else if (Unigine.Input.IsMouseButtonUp(Unigine.Input.MOUSE_BUTTON.LEFT))
		{
			if (mousePressed)
			{
				view.MouseButtonUp(mouseX, mouseY, Noesis.MouseButton.Left);
				mousePressed = false;
			}
		}
	}

	public void Shutdown()
	{
		renderTexture.Clear();
		hud.DeleteForce();

		xaml.Dispose();
		if (view != null)
		{
			view.Renderer.Shutdown();
			view.Dispose();
		}

		device.Dispose();
		Noesis.GUI.Shutdown();
	}
}