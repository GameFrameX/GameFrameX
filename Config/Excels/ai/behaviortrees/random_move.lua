 return {
	id=10002,
	name="random move",
	desc="demo behaviour tree haha",
	executor="SERVER",
	blackboard_id="demo",
	root=
	{
		__type__ = "Sequence",
		id=1,
		node_name="test",
		desc="root",
		services=
		{

		},
		decorators=
		{
			{ __type__="UeLoop", id=3,node_name="",flow_abort_mode="SELF", num_loops=0,infinite_loop=true,infinite_loop_timeout_time=-1,},
		},
		children =
		{
			{__type__="UeWait", id=30,node_name="", ignore_restart_self=false,wait_time=1,random_deviation=0.5, services={},decorators={},},
			{__type__="MoveToRandomLocation", id=75,node_name="", ignore_restart_self=false,origin_position_key="x5",radius=30, services={},decorators={}},
			--{__type__="DebugPrint", id=76,node_name="", ignore_restart_self=false,text="======= bt debug print ===", services={},decorators={}},
		},
	},
 }