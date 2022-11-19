← [Author Guide](author-guide.md)

# Built-in Patches

The following is a list of built-in patches:

* [Billboard](#billboard)
* [BobberBar](#bobberbar)
* [CarpenterMenu](#carpentermenu)
* [CharacterCustomization](#charactercustomization)
* [CoopMenu](#coopmenu)
* [DayTimeMoneyBox](#daytimemoneybox)
* [DrawHud](#drawhud)
* [DrawTextWithShadow](#drawtextwithshadow)
* [ExitPage](#exitpage)
* [ForgeMenu](#forgemenu)
* [ItemTooltips](#itemtooltips)
* [LetterViewerMenu](#letterviewermenu)
* [LevelUpMenu](#levelupmenu)
* [LoadGameMenu](#loadgamemenu)
* [MineElevatorMenu](#mineelevatormenu)
* [MoneyDial](#moneydial)
* [OptionsDropDown](#optionsdropdown)
* [QuestLog](#questlog)
* [ShopMenu](#shopmenu)
* [SkillsPage](#skillspage)
* [SpecialOrdersBoard](#specialordersboard)
* [TutorialMenu](#tutorialmenu)

# Billboard

<table>
<tr>
<th>Id</th>
<td>Billboard</td>
</tr>
<tr>
<th>Variables</th>
<td>
<table>
<tr><th>Name</th><th>Default Value</th></tr>
<tr><td>$BillboardHover</td><td>$ButtonHover</td></tr>
<tr><td>$BillboardText</td><td><i>none</i></td></tr>
<tr><td>$CalendarDim</td><td><i>none</i></td></tr>
<tr><td>$CalendarToday</td><td><i>none</i></td></tr>
</table>
<tr>
<th>Target</th>
<td>
<table>
<tr><th>Class</th><th>Method</th></tr>
<tr><td>StardewValley.Menus.Billboard</td><td>draw</td></tr>
</table>
</td>
</tr>
</table>

The Billboard patch, despite only affecting the Billboard class, affects two
separate menus in the game:

1. The Calendar
2. The Help Wanted billboard

<table>
<tr>
<th>Calendar</th>
<th>Billboard</th>
</tr>
<tr>
<td>

![](docs/Calendar-Normal.png)

</td>
<td>

![](docs/Billboard-Normal.png)

</td>
</tr>
</table>

## Variable: `$BillboardHover`

The billboard hover variable affects the color of the "Accept Quest" button
when the user is hovering over it with their mouse. The following table
demonstrates the button in its normal state and the button in its hover
state:

<table>
<tr><th>Normal</th><th>Hover</th></tr>
<tr>
<td>

![](docs/AcceptQuest-Normal.png)

</td>
<td>

![](docs/AcceptQuest-HoverNormal.png)

</td>
</tr>
</table>

The texture is being colored with `White` when not hovered, and `LightPink`
when hovered. As you can see, the texture already *has* color and the
`LightPink` is just altering the existing hue.

The following demonstrates how the button's hover state changes if
the `$BillboardHover` variable is set to `LimeGreen`:

![](docs/AcceptQuest-HoverGreen.png)

This variable has no effect on the Calendar menu. Only on the Help Wanted
billboard menu.


## Variable: `$BillboardText`

The billboard text variable affects the color of the following text:

* The season and year on the Calendar.
* The description of the quest on the Help Wanted billboard
* The text on the Accept Quest button on the Help Wanted billboard

The following images compare the appearance of the variable with its default
value and the appearance of the variable when set to `LimeGreen`:

<table>
<tr>
<th></th>
<th>Normal</th>
<th>LimeGreen</th>
</tr>
<tr>
<th>Calendar</th>
<td>

![](docs/Calendar-Normal.png)

</td>
<td>

![](docs/Calendar-Green.png)

</td>
</tr>
<tr>
<th>Billboard</th>
<td>

![](docs/Billboard-Normal.png)

</td>
<td>

![](docs/Billboard-Green.png)

</td>
</tr>
</table>

You'll note that all other text within the two menus is either baked into
textures or drawn in a different way that isn't affected by this variable.
As such, the variable doesn't affect most visible text.


## Variable: `$CalendarDim`

The calendar dim variable affects the color of the square that is drawn over
previous days. The default color used by the game is `Gray` and it's drawn
with 25% opacity.

The following images compare the appearance of the variable with its default
value and the appearance of the variable when set to `LimeGreen`:

<table>
<tr>
<th>Normal</th>
<th>LimeGreen</th>
</tr>
<tr>
<td>

![](docs/Calendar-Normal.png)

</td>
<td>

![](docs/Calendar-Dim-Green.png)

</td>
</tr>
</table>


## Variable: `$CalendarToday`

The calendar today variable affects the color of the pulsating square that is
drawn around the current day. The default color used by the game is `Blue`.

The following images compare the appearance of the variable with its default
value and the appearance of the variable when set to `LimeGreen`:

<table>
<tr>
<th>Normal</th>
<th>LimeGreen</th>
</tr>
<tr>
<td>

![](docs/Calendar-Today-Normal.png)

</td>
<td>

![](docs/Calendar-Today-Green.png)

</td>
</tr>
</table>


# BobberBar

# CarpenterMenu

# CharacterCustomization

# CoopMenu

# DayTimeMoneyBox

# DrawHud

# DrawTextWithShadow

# ExitPage

# ForgeMenu

# ItemTooltips

# LetterViewerMenu

# LevelUpMenu

# LoadGameMenu

# MineElevatorMenu

# MoneyDial

# OptionsDropDown

# QuestLog

# ShopMenu

# SkillsPage

# SpecialOrdersBoard

# TutorialMenu
