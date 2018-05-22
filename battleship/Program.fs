open GameCore
open View

[<EntryPoint>]
let main _ =
    use game = new GameCore<Model.GameModel>(View.resolution, View.assetsToLoad, Controller.advanceGame, View.getView)
    game.Run ()
    0
