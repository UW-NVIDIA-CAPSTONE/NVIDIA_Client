   M S B A M L   `   `   `  ����J  FWpfApplication1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:WpfApplication1WpfApplication1  5   	       WpfApplication1.second   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:WpfApplication1     5   	     � Title$  Video Walkthrough��5   	   $��560.605��6#   $��729.444��64   .��5	      � $��	0,0,271,3q�6    � RowDefinitions 5
   
   �� 5      $	��43* �6   �� 5      $
��210* �6   .+�5   
   (  "System.Windows.Controls.WebBrowser  -   5          Name$ WebBrowser1��5      $
��Left=�6*   $	��446��6E   $��7,13,-260,0q�6R   $	��Top=�6g   $	��703��6   $��2��6�    J� RenderTransformOrigin$ 0.5,0.5&�6�   z�5      V� 5      .~�5      �� �� 5      �� 5       �� Angle$ -0.175Q�6&   T� 5      5      5      �� 5   
   -   5         ��Name$ 
Skip_F_Btn��5      $�� ��6%   $��0,383,-264,0q�60   $��1��6F   $��81��6S   $��47��6^   $��Right=�6�   $	��Top=�6�   ��5      �� 5       �� ImageSource$ Res/fast-forward.png��6    j� Stretch$ Uniform=�6@   5      �� 5   
   -   5        $ 
Skip_B_Btn��5      $�� ��6%   $��
10,383,0,0q�60   $��1��6D   $��82��6Q   $��47��6\   $	��Top=�6�   $
��Left=�6�   ��5      �� 5      $ Uniform=�6   $ Res/back.png��6/   5!      �� 5"   
   -   5         ��Name$ ResultLabel��5"      $��Label��6%   $��Center=�65   $��205,383,-69,0q�6R   $	��Top=�6i   $��1��6�   $	��314��6�   5$      5%      