   M S B A M L   `   `   `  ����?  ;IMUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:IMClient.Views.ControlsIMClient.Views.Controls  5      .    (IMClient.Views.Controls.AnimationLoading   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      7local%clr-namespace:IMClient.Views.Controls     5        � 	Resources  5         Loading g� RepeatBehavior g� Duration W� From	 W� To �� 
TargetName �� TargetProperty��%�   &	        �� 5	   
   .��5
      W� $ Forever��6    $ 0:0:1D�69   $ 0X�5       $	 360X�6)   $ rotate��62   $ Angle�6Q   5      5       � Triggers 5      :� 5   
    :� RoutedEvent$ UserControl.Loaded��6   .[�5      �� -   5        	 ��Name$	 BeginLoaded��5      #��[  63   5      :� 5   
   $ UserControl.Unloaded��6   .[�5      �� 
 �� BeginStoryboardName$
 BeginLoaded��6   5      .��5      (� .��5   
   � -   5         �Name$ 
gridLayout��5       J� RenderTransformOrigin$ 0.5,0.5&�6#   $	��100��6C   $	��100��6O   $��10q�6\   .+�5      A� $��10��6   ���	#FFCCCCCC6+   F� 5      %����@KL ����������� �  � 	$��10��65   
�������$ 0.5,0.5&�6Y   z�5      �� 5      -   5        
rotate��5   &    J� OpacityMask 5      �� 5       �� 
StartPoint$	 0,1&�6*    �� EndPoint$	 1,1&�6;   .��5      �  � Color$ White��6'    � Offset$	 0.5Q�65   � 5       $ Transparent��6'   $	 0.5Q�6;   5!      5#      5$      5%      5�      