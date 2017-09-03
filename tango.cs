//	tan·go
//	/ˈtaNGɡō/

$Tango::Debug = false;

//If an add-on already has an equal or newer version of Tango, let's stop running the script.
//Assuming we're not in debug mode, of course ...
if( !$Tango::Debug && $Tango::Version > 0 && $Tango::Version >= 040 )
{
	return;
}

//Set the version number.
$Tango::Version = 040; // 0.4.0

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
	%this.tangoMoveStartTime = getSimTime();
	%this.tangoMoveEndTime = getSimTime() + %ms;
	%this.tangoMoveTick(%easeMode[x], %easeMode[y], %mypos, %dist[x], %dist[y]);
}

//Called every 33ms while the GuiControl moves to its destination.
//@param	GuiControl this
//@param	string mode[x]
//@param	string mode[y]
//@param	Vector2F startpos
//@param	int dist[x]
//@param	int dist[y]
//@param	int ms
//@see	GuiControl::tangoMoveTo
//@private
function GuiControl::tangoMoveTick(%this, %modex, %modey, %startpos, %distx, %disty)
{
	cancel(%this.tangoMoveTick);

	%currTime = getSimTime() - %this.tangoMoveStartTime;
	%ms = %this.tangoMoveEndTime - %this.tangoMoveStartTime;

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

	%func[x] = %func[y] = "tangoEaseLinear";

	if(isFunction(%ifn = "tangoEase" @ %mode[x]))
		%func[x] = %ifn;
	if(isFunction(%ifn = "tangoEase" @ %mode[y]))
		%func[y] = %ifn;

	%pos[x] = call(%func[x], %currTime, 0, %dist[x], %ms);
	%pos[y] = call(%func[y], %currTime, 0, %dist[y], %ms);

	tngE("pos[x] = " @ %pos[x]);
	tngE("pos[y] = " @ %pos[y]);

	%x = getWord(%startpos, 0) + %pos[x];
	%y = getWord(%startpos, 1) + %pos[y];
	%w = getWord(%this.extent, 0);
	%h = getWord(%this.extent, 1);

	%this.resize(%x, %y, %w, %h);

	%this.tangoMoveTick = %this.schedule( 1, tangoMoveTick, %mode[x], %mode[y], %startpos, %distx, %disty );
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
	%this.tangoScaleStartTime = getSimTime();
	%this.tangoScaleEndTime = getSimTime() + %ms;
	%this.tangoScaleTick(%easeMode[x], %easeMode[y], %myscale, %dist[x], %dist[y]);
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
function GuiControl::tangoScaleTick(%this, %modex, %modey, %startscale, %distx, %disty)
{
	cancel(%this.tangoScaleTick);

	%currTime = getSimTime() - %this.tangoScaleStartTime;
	%ms = %this.tangoScaleEndTime - %this.tangoScaleStartTime;

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

	%func[x] = %func[y] = "tangoEaseLinear";

	if(isFunction(%ifn = "tangoEase" @ %mode[x]))
		%func[x] = %ifn;
	if(isFunction(%ifn = "tangoEase" @ %mode[y]))
		%func[y] = %ifn;

	%pos[x] = call(%func[x], %currTime, 0, %dist[x], %ms);
	%pos[y] = call(%func[y], %currTime, 0, %dist[y], %ms);

	%x = getWord(%this.position, 0);
	%y = getWord(%this.position, 1);
	%w = getWord(%startscale, 0) + %pos[x];
	%h = getWord(%startscale, 1) + %pos[y];

	%this.resize(%x, %y, %w, %h);

	%this.tangoScaleTick = %this.schedule( 1, tangoScaleTick, %mode[x], %mode[y], %startscale, %distx, %disty );
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
	%flip = false;
	if (%c < 0)
	{
		%c = mAbs( %c );
		%flip = true;
	}

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

	%r = %a * mPow( 2, -10 * %t ) * mSin( ( %t * %d - %s ) * ( 2 * %pi ) / %p ) + %c + %b;

	return %flip ? -%r : %r;
}
