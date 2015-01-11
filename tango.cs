//	tan·go
//	/ˈtaNGɡō/
//
//	noun
//	1.	a ballroom dance originating in Buenos Aires, characterized by marked rhythms
//		and postures and abrupt pauses.
//
//	2.	a code word representing the letter T, used in voice communication by radio.
//
//	verb
//	1.	dance the tango.

$Tango::Debug = false;

//If an add-on already has an equal or newer version of Tango, let's stop running the script.
//Assuming we're not in debug mode, of course ...
if( !$Tango::Debug && $Tango::Version > 0 && $Tango::Version >= 030 )
{
	return;
}

//Set the version number.
$Tango::Version = 030; // 0.3.0

//A function to echo something in a readable format for debugging.
//@param	string string	The message to output.
//@deprecated
function tngE(%string)
{
	if(!$Tango::Debug) return;
	if($tngEcf $= "") $tngEcf = "n/a";
	echo("\c4TANGO Debug @" SPC getSimTime() SPC "(" @ $tngEcf @ "):" SPC %string);
}

//A function to snap a GuiControl to a desired position.
//@param	GuiControl this
//@param	Vector2F xy	The position to set the GuiControl to, relative to its parent.
//This function does not support seamless movement or easing. It is an instantaneous modifier.
//@see	GuiControl::tangoMoveTo
function GuiControl::tangoSetPosition(%this, %xy)
{
	$tngEcf = "tangoSetPosition";
	tngE("Running.");

	cancel(%this.tangoMoveTick);

	%xy = getWords( VectorAdd( %xy, "0 0 0" ), 0, 1 );

	%mypos = %this.position;
	if(%mypos $= %xy)
	{
		tngE("Already here!");
		return;
	}

	%x = getWord(%xy, 0);
	%y = getWord(%xy, 1);
	%w = getWord(%this.extent, 0);
	%h = getWord(%this.extent, 1);

	%this.resize(%x, %y, %w, %h);
}

//A function to smoothly move a GuiControl to a specified position, with optional easing.
//Easing modes are: <i>quad , sine , expo , circ , cube , quart , elastic , <b>linear</b></i>.
//@param	GuiControl this
//@param	Vector2F xy	The position to move the GuiControl to, relative to its parent.
//@param	int ms	(1 - 30000) The time, in milliseconds, it should take for the GuiControl to move.
//@param	string easeModeX	(Optional) The easing mode for the X axis. Should this not be specified, the GuiControl will move linearly.
//@param	string easeModeY	(Optional) The easing mode for the Y axis. Should this not be specified, it will use the X easing mode.
//@see	GuiControl::tangoSetPosition
function GuiControl::tangoMoveTo(%this, %xy, %ms, %easeModeX, %easeModeY)
{
	$tngEcf = "tangoMoveTo";
	tngE("Running.");

	cancel(%this.tangoMoveTick);

	%xy = getWords( VectorAdd( %xy, "0 0 0" ), 0, 1 );
	%ms = mClamp(%ms, 1, 30000);

	%mypos = %this.position;
	if(%mypos $= %xy)
	{
		tngE("Already here!");
		return;
	}

	%at[x] = getWord(%mypos, 0);
	%at[y] = getWord(%mypos, 1);
	tngE("at[x] = " @ %at[x]);
	tngE("at[y] = " @ %at[y]);

	%to[x] = getWord(%xy, 0);
	%to[y] = getWord(%xy, 1);
	tngE("to[x] = " @ %to[x]);
	tngE("to[y] = " @ %to[y]);

	%dist[x] = -( %at[x] - %to[x] );
	%dist[y] = -( %at[y] - %to[y] );
	tngE("dist[x] = " @ %dist[x]);
	tngE("dist[y] = " @ %dist[y]);

	tngE("Starting.");
	if(%easeModeY $= "") %easeModeY = %easeModeX;
	%this.tangoMoveTick(%easeMode[x], %easeMode[y], %mypos, %dist[x], %dist[y], %ms, 0);
}

//Called every 33ms while the GuiControl moves to its destination.
//@param	GuiControl this
//@param	string mode[x]
//@param	string mode[y]
//@param	Vector2F startpos
//@param	int dist[x]
//@param	int dist[y]
//@param	int ms
//@param	int currTime
//@see	GuiControl::tangoMoveTo
//@private
function GuiControl::tangoMoveTick(%this, %modex, %modey, %startpos, %distx, %disty, %ms, %currTime)
{
	cancel(%this.tangoMoveTick);

	if(%currTime > %ms)
	{
		%x = getWord(%startpos, 0) + %dist[x];
		%y = getWord(%startpos, 1) + %dist[y];
		%w = getWord(%this.extent, 0);
		%h = getWord(%this.extent, 1);
	
		%this.resize(%x, %y, %w, %h);

		$tngEcf = "tangoMoveTick";
		tngE("Done!" SPC %this.position SPC %distx SPC %disty);
		return;
	}
	
	switch$(%mode[x])
	{
		case "quad":
			%pos[x] = tangoEaseQuad(%currTime, 0, %dist[x], %ms);
		case "sine":
			%pos[x] = tangoEaseSine(%currTime, 0, %dist[x], %ms);
		case "expo":
			%pos[x] = tangoEaseExpo(%currTime, 0, %dist[x], %ms);
		case "circ":
			%pos[x] = tangoEaseCirc(%currTime, 0, %dist[x], %ms);
		case "cube":
			%pos[x] = tangoEaseCube(%currTime, 0, %dist[x], %ms);
		case "quart":
			%pos[x] = tangoEaseQuart(%currTime, 0, %dist[x], %ms);
		case "elastic":
			if( %dist[x] < 0 )
				%pos[x] = -tangoEaseElastic(%currTime, 0, mAbs(%dist[x]), %ms);
			else
				%pos[x] = tangoEaseElastic(%currTime, 0, %dist[x], %ms);
		default:
			%pos[x] = tangoEaseLinear(%currTime, 0, %dist[x], %ms);
	}
	switch$(%mode[y])
	{
		case "quad":
			%pos[y] = tangoEaseQuad(%currTime, 0, %dist[y], %ms);
		case "sine":
			%pos[y] = tangoEaseSine(%currTime, 0, %dist[y], %ms);
		case "expo":
			%pos[y] = tangoEaseExpo(%currTime, 0, %dist[y], %ms);
		case "circ":
			%pos[y] = tangoEaseCirc(%currTime, 0, %dist[y], %ms);
		case "cube":
			%pos[y] = tangoEaseCube(%currTime, 0, %dist[y], %ms);
		case "quart":
			%pos[y] = tangoEaseQuart(%currTime, 0, %dist[y], %ms);
		case "elastic":
			if( %dist[y] < 0 )
				%pos[y] = -tangoEaseElastic(%currTime, 0, mAbs(%dist[y]), %ms);
			else
				%pos[y] = tangoEaseElastic(%currTime, 0, %dist[y], %ms);
		default:
			%pos[y] = tangoEaseLinear(%currTime, 0, %dist[y], %ms);
	}

	%x = getWord(%startpos, 0) + %pos[x];
	%y = getWord(%startpos, 1) + %pos[y];
	%w = getWord(%this.extent, 0);
	%h = getWord(%this.extent, 1);

	%this.resize(%x, %y, %w, %h);

	%this.tangoMoveTick = %this.schedule( 33, tangoMoveTick, %mode[x], %mode[y], %startpos, %distx, %disty, %ms, %currTime += 33 );
}

//A function to smoothly scale a GuiControl to a specified extent, with optional easing.
//@param	GuiControl this
//@param	Vector2F xy	The extent to scale the GuiControl to.
//@param	int ms	(1 - 30000) The time, in milliseconds, it should take for the GuiControl to scale.
//@param	string easeModeX	(Optional) The easing mode for the X axis. Should this not be specified, the GuiControl will scale linearly.
//@param	string easeModeY	(Optional) The easing mode for the Y axis. Should this not be specified, it will use the X easing mode.
//@see	GuiControl::tangoMoveTo
function GuiControl::tangoScaleTo(%this, %xy, %ms, %easeModeX, %easeModeY)
{
	$tngEcf = "tangoScaleTo";
	tngE("Running.");

	cancel(%this.tangoScaleTick);

	%xy = getWords( VectorAdd( %xy, "0 0 0" ), 0, 1 );
	%ms = mClamp(%ms, 1, 30000);

	%myscale = %this.extent;
	if(%myscale $= %xy)
	{
		tngE("Already here!");
		return;
	}

	%at[x] = getWord(%myscale, 0);
	%at[y] = getWord(%myscale, 1);
	tngE("at[x] = " @ %at[x]);
	tngE("at[y] = " @ %at[y]);

	%to[x] = getWord(%xy, 0);
	%to[y] = getWord(%xy, 1);
	tngE("to[x] = " @ %to[x]);
	tngE("to[y] = " @ %to[y]);

	%dist[x] = -( %at[x] - %to[x] );
	%dist[y] = -( %at[y] - %to[y] );
	tngE("dist[x] = " @ %dist[x]);
	tngE("dist[y] = " @ %dist[y]);

	tngE("Starting.");
	if(%easeModeY $= "") %easeModeY = %easeModeX;
	%this.tangoScaleTick(%easeMode[x], %easeMode[y], %myscale, %dist[x], %dist[y], %ms, 0);
}

//Called every 33ms while the GuiControl scales to its destination.
//@param	GuiControl this
//@param	string mode[x]
//@param	string mode[y]
//@param	Vector2F startscale
//@param	int dist[x]
//@param	int dist[y]
//@param	int ms
//@param	int currTime
//@see	GuiControl::tangoMoveTo
//@private
function GuiControl::tangoScaleTick(%this, %modex, %modey, %startscale, %distx, %disty, %ms, %currTime)
{
	cancel(%this.tangoScaleTick);

	if(%currTime > %ms)
	{
		%x = getWord(%this.position, 0);
		%y = getWord(%this.position, 1);
		%w = getWord(%startscale, 0) + %dist[x];
		%h = getWord(%startscale, 1) + %dist[y];
	
		%this.resize(%x, %y, %w, %h);

		$tngEcf = "tangoScaleTick";
		tngE("Done!" SPC %this.position SPC %distx SPC %disty);
		return;
	}
	
	switch$(%mode[x])
	{
		case "quad":
			%pos[x] = tangoEaseQuad(%currTime, 0, %dist[x], %ms);
		case "sine":
			%pos[x] = tangoEaseSine(%currTime, 0, %dist[x], %ms);
		case "expo":
			%pos[x] = tangoEaseExpo(%currTime, 0, %dist[x], %ms);
		case "circ":
			%pos[x] = tangoEaseCirc(%currTime, 0, %dist[x], %ms);
		case "cube":
			%pos[x] = tangoEaseCube(%currTime, 0, %dist[x], %ms);
		case "quart":
			%pos[x] = tangoEaseQuart(%currTime, 0, %dist[x], %ms);
		case "elastic":
			if( %dist[x] < 0 )
				%pos[x] = -tangoEaseElastic(%currTime, 0, mAbs(%dist[x]), %ms);
			else
				%pos[x] = tangoEaseElastic(%currTime, 0, %dist[x], %ms);
		default:
			%pos[x] = tangoEaseLinear(%currTime, 0, %dist[x], %ms);
	}
	switch$(%mode[y])
	{
		case "quad":
			%pos[y] = tangoEaseQuad(%currTime, 0, %dist[y], %ms);
		case "sine":
			%pos[y] = tangoEaseSine(%currTime, 0, %dist[y], %ms);
		case "expo":
			%pos[y] = tangoEaseExpo(%currTime, 0, %dist[y], %ms);
		case "circ":
			%pos[y] = tangoEaseCirc(%currTime, 0, %dist[y], %ms);
		case "cube":
			%pos[y] = tangoEaseCube(%currTime, 0, %dist[y], %ms);
		case "quart":
			%pos[y] = tangoEaseQuart(%currTime, 0, %dist[y], %ms);
		case "elastic":
			if( %dist[y] < 0 )
				%pos[y] = -tangoEaseElastic(%currTime, 0, mAbs(%dist[y]), %ms);
			else
				%pos[y] = tangoEaseElastic(%currTime, 0, %dist[y], %ms);
		default:
			%pos[y] = tangoEaseLinear(%currTime, 0, %dist[y], %ms);
	}

	%x = getWord(%this.position, 0);
	%y = getWord(%this.position, 1);
	%w = getWord(%startscale, 0) + %pos[x];
	%h = getWord(%startscale, 1) + %pos[y];

	%this.resize(%x, %y, %w, %h);

	%this.tangoScaleTick = %this.schedule( 33, tangoScaleTick, %mode[x], %mode[y], %startscale, %distx, %disty, %ms, %currTime += 33 );
}

//Determines an offset position for a linear movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
function tangoEaseLinear(%t, %b, %c, %d)
{
	return %c * %t / %d + %b;
}

//Determines an offset position for a quadratic eased movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseQuad(%t, %b, %c, %d)
{
	%t /= %d / 2;
	if ( %t < 1 ) return %c / 2 * %t * %t + %b;
	%t--;
	return -%c / 2 * ( %t * ( %t - 2 ) - 1 ) + %b;
}

//Determines an offset position for a sinusoidal eased movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseSine(%t, %b, %c, %d)
{
	return -%c / 2 * ( mCos( $pi * %t / %d ) - 1 ) + %b;
}

//Determines an offset position for an exponential eased movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseExpo(%t, %b, %c, %d)
{
	%t /= %d / 2;
	if ( %t < 1 ) return %c / 2 * mPow( 2, 10 * ( %t - 1 ) ) + %b;
	%t--;
	return %c / 2 * ( -mPow( 2, -10 * %t ) + 2 ) + %b;
}

//Determines an offset position for a circular eased movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseCirc(%t, %b, %c, %d)
{
	%t /= %d / 2;
	if ( %t < 1 ) return -%c / 2 * ( mSqrt ( 1 - %t * %t ) - 1 ) + %b;
	%t -= 2;
	return %c / 2 * ( mSqrt ( 1 - %t * %t ) + 1 ) + %b;
}

//Determines an offset position for a cubic eased movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseCube(%t, %b, %c, %d)
{
	%t /= %d / 2;
	if ( %t < 1 ) return %c / 2 * %t * %t * %t + %b;
	%t -= 2;
	return %c / 2 * ( %t * %t * %t + 2 ) + %b;
}

//Determines an offset position for a quartic eased movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseQuart(%t, %b, %c, %d)
{
	%t /= %d / 2;
	if ( %t < 1 ) return %c / 2 * %t * %t * %t * %t + %b;
	%t -= 2;
	return -%c / 2 * ( %t * %t * %t * %t - 2 ) + %b;
}

//Determines an offset position for an elastic movement along an axis.
//@return	float	The algebraic output of the new offset position.
//@param	int t	The time, in milliseconds, since the beginning.
//@param	int b	The starting point.
//@param	int c	The ending point.
//@param	int d	The ending time, also in milliseconds.
//@see	tangoEaseLinear
function tangoEaseElastic(%t, %b, %c, %d)
{
	%s = 1.70158; %p = 0; %a = %c;

	if( %t == 0 ) return %b;
	if( ( %t /= %d ) == 1) return %b + %c;
	if( !%p ) %p = %d * 0.3;

	if( %a < mAbs( %c ) )
	{
		%a = %c;
		%s = %p / 4;
	}
	else
	{
		%s = %p / ( 2 * %pi) * mAsin( %c / %a );
	}

	return %a * mPow( 2, -10 * %t ) * mSin( ( %t * %d - %s ) * ( 2 * %pi ) / %p ) + %c + %b;
}
