using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinBotForms.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(KeybordAwarePageRenderer))]
namespace XamarinBotForms.iOS
{
	[Preserve(AllMembers = true)]
	public class KeybordAwarePageRenderer: PageRenderer
	{
		NSObject _keyboardShowObserver;
		NSObject _keyboardHideObserver;
		bool _pageWasShiftedUp;
		double _activeViewBottom;
		bool _isKeyboardShown;

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			if (!(Element is ContentPage page) || page.Content is ScrollView)
				return;

			RegisterForKeyboardNotifications();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			UnregisterForKeyboardNotifications();
		}

		void RegisterForKeyboardNotifications()
		{
			if (_keyboardShowObserver == null)
				_keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardShow);
			if (_keyboardHideObserver == null)
				_keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardHide);
		}

		void UnregisterForKeyboardNotifications()
		{
			_isKeyboardShown = false;
			if (_keyboardShowObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardShowObserver);
				_keyboardShowObserver.Dispose();
				_keyboardShowObserver = null;
			}

			if (_keyboardHideObserver != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardHideObserver);
				_keyboardHideObserver.Dispose();
				_keyboardHideObserver = null;
			}
		}

		protected virtual void OnKeyboardShow(NSNotification notification)
		{
			if (!IsViewLoaded || _isKeyboardShown)
				return;

			_isKeyboardShown = true;
			var activeView = View.FindFirstResponder();

			if (activeView == null)
				return;

			var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
			var isOverlapping = activeView.IsKeyboardOverlapping(View, keyboardFrame);

			if (!isOverlapping)
				return;

			if (isOverlapping)
			{
				_activeViewBottom = activeView.GetViewRelativeBottom(View);
				ShiftPageUp(keyboardFrame.Height, _activeViewBottom);
			}
		}

		void OnKeyboardHide(NSNotification notification)
		{
			if (!IsViewLoaded)
				return;

			_isKeyboardShown = false;
			var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);

			if (_pageWasShiftedUp)
				ShiftPageDown(keyboardFrame.Height, _activeViewBottom);
		}

		void ShiftPageUp(nfloat keyboardHeight, double activeViewBottom)
		{
			var pageFrame = Element.Bounds;

			var newY = pageFrame.Y + CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);

			Element.LayoutTo(new Rectangle(pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));

			_pageWasShiftedUp = true;
		}

		void ShiftPageDown(nfloat keyboardHeight, double activeViewBottom)
		{
			var pageFrame = Element.Bounds;

			var newY = pageFrame.Y - CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);

			Element.LayoutTo(new Rectangle(pageFrame.X, newY,
				pageFrame.Width, pageFrame.Height));

			_pageWasShiftedUp = false;
		}

		double CalculateShiftByAmount(double pageHeight, nfloat keyboardHeight, double activeViewBottom)
		{
			return (pageHeight - activeViewBottom) - keyboardHeight;
		}
	}



	public static class ViewExtensions
	{
		public static UIView FindFirstResponder(this UIView view)
		{
			if (view.IsFirstResponder)
			{
				return view;
			}
			foreach (UIView subView in view.Subviews)
			{
				var firstResponder = subView.FindFirstResponder();
				if (firstResponder != null)
					return firstResponder;
			}
			return null;
		}

		public static double GetViewRelativeBottom(this UIView view, UIView rootView)
		{
			var viewRelativeCoordinates = rootView.ConvertPointFromView(view.Frame.Location, view);
			var activeViewRoundedY = Math.Round(viewRelativeCoordinates.Y, 2);

			return activeViewRoundedY + view.Frame.Height;
		}

		public static bool IsKeyboardOverlapping(this UIView activeView, UIView rootView, CGRect keyboardFrame)
		{
			var activeViewBottom = activeView.GetViewRelativeBottom(rootView);
			var pageHeight = rootView.Frame.Height;
			var keyboardHeight = keyboardFrame.Height;

			var isOverlapping = activeViewBottom >= (pageHeight - keyboardHeight);

			return isOverlapping;
		}
	}
}