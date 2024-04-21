Proto文件名称规则：
 	文件名-协议开始-协议结束.proto
	示例：BattleMessage-1000-2000.proto
	

消息规则：
客户端到服务器消息 ：
 message C2S_Test{ 

}

服务器到客户端消息 ： 
message S2C_Test{

}

C2S_Test --- Opcode = 10
S2C_Test --- Opcode = 10

C2S 和 S2C 协议号会在区间内自动递增。 
协议名称相同则 Opcode相同