namespace TalBot

type Message = { destination: string; sender: string; text : string; icon: string }
type MessageOption = Message option