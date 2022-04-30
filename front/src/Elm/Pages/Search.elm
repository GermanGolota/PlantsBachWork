module Pages.Search exposing (..)

import Html exposing (Html, div)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import NavBar exposing (navView)



--model


type alias Model =
    ModelBase View


type alias View =
    {}



--update


type Msg
    = NoOp


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    case m of
        Authorized auth model ->
            ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands
--view


view : Model -> Html Msg
view model =
    viewBase viewMain model


viewMain : AuthResponse -> View -> Html Msg
viewMain resp model =
    navView resp.username resp.roles (Just NavBar.searchLink) (pageView model)


pageView : View -> Html Msg
pageView viewType =
    div [] []


init : Maybe AuthResponse -> ( Model, Cmd Msg )
init resp =
    initBase [ Producer, Consumer, Manager ] ( {}, Cmd.none ) resp


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
