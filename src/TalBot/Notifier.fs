module Notifier

open BotHelper

let getNotifications () =
    getMessagesFromNotificationPlugins ()