   M S B A M L   `   `   `  ����   ProPresentation     proPresentation.HelpWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$
  Help�� � ShowInTaskbar . $	��700��$	��600�� � Icon$  Images\ProPresentation.ico�� � 
ResizeMode$ NoResize=� � 	Resources    
LinkButton �� 
TargetType �� Property �� Value �� 
TargetType	 �� Triggers
 P� Property P� Value��%�   &	        �� " ��.���  � �� �� " ��.R��� $��	Underline{��� ��  � ��
 ��  �	 P� 
 �@J�IsMouseOver . . ���  � ��
 �  ��.��� -   $��helpWindowGrid�� � RowDefinitions �� $	��80* ��� $
��435* ��� $	��54* �.+��� -   $��topTextBlock��$ ��Please select your option.��$
��Left=�$��20q�$��25(��� -   $��startContentStackPanel�� �� Orientation$ Vertical=�$��1��$y�Visible=�.+��� -   $��part1��$ ��How to create new project.��$��20,10q�$��15(�#��[  �� -   $��part2��$#��How to open existing project.��$��20,10q�$��15(�#��[  �� -   $��part3��$2��,Steps after creating or opening new project.��$��20,10q�$��15(�#��[  �� -   $��part4��$��Starting Presentation.��$��20,10q�$��15(�#��[  �� -   $��part5��$��Keyboard shortcuts.��$��20,10q�$��15(�#��[  �� -	   $��part6��$��Important things to note.��$��20,10q�$��15(�#��[  ���	#FFDCAB05�� -
   $��bottomStackPanel��$ 
Horizontal=�$��2��$��Right=�$	��213��$y�Hidden=�.+��� -   $
��menu��$
��Menu��$��2��$��27��$��40��$��	10,0,10,0q�$��Right=��� -   $��previous��$��Previous��$��2��$��27��$��60��$��	10,0,10,0q�$��Right=��� -   $
��next��$
��Next��$��2��$��27��$��40��$��	10,0,10,0q�$��Right=��� -   $��imageBorder��$��2q�$��1��
���   �$	��354��$	��520��$
��Left=�$	��Top=�$��
38,140,0,0q�$y�	Collapsed=��� -    ��Name$ imageBox��$
��Left=�$	��348��$��
41,143,0,0q�$��1��$	��Top=�$	��514��$y�	Collapsed=��� -    ��Name$ stepsTextBlock��$
��Left=�$	��125��$��	38,10,0,0q�$��1��$	��Top=�$	��514��$y�	Collapsed=�$
��Wrap=�$��19(�