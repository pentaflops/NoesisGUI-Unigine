namespace UnigineApp
{
	class AppSystemLogic : Unigine.SystemLogic
	{
		private NoesisGUI noesisGUI;

		public AppSystemLogic()
		{
		}

		public override bool Init()
		{
			noesisGUI = new NoesisGUI();
			noesisGUI.Initialize();

			return true;
		}

		public override bool Update()
		{
			noesisGUI.Update();

			return true;
		}

		public override bool PostUpdate()
		{
			return true;
		}

		public override bool Shutdown()
		{
			noesisGUI.Shutdown();

			return true;
		}
	}
}