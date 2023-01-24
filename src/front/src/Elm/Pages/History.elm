module Pages.History exposing (..)

import Bootstrap.Utilities.Flex as Flex
import Html exposing (Html, div, text)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), MsgBase, UserRole(..), baseApplication, initBase, updateBase)
import NavBar exposing (plantsLink, viewNav)
import Json.Decode exposing (errorToString)



--model


type alias Model =
    ModelBase View


type View
    = Valid ViewValue
    | Invalid


type alias ViewValue =
    { aggregate : AggregateDescription
    }


type alias AggregateDescription =
    { name : String
    , id : String
    }



--update


type LocalMsg
    = NoOp


type alias Msg =
    MsgBase LocalMsg


update : LocalMsg -> Model -> ( Model, Cmd Msg )
update msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model ->
            let
                authed =
                    Authorized auth

                updateModel newModel =
                    ( authed newModel, Cmd.none )
            in
            case msg of
                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands
--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        Valid agg ->
            div [] [ text <| "Viewing aggregate " ++ agg.aggregate.name ++ " " ++ agg.aggregate.id ]

        Invalid ->
            div [] [ text "Failed to load aggregate" ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        getAggregate =
            D.decodeValue (D.map2 AggregateDescription (D.field "id" D.string) (D.field "name" D.string)) flags


        initialState =
            case getAggregate of
                Ok agg ->
                    Valid { aggregate = agg }

                Err err ->
                    Debug.log (errorToString err) Invalid
    in
    initBase [ Producer, Consumer, Manager ] initialState (\res -> Cmd.none) resp



subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = updateBase update
        , subscriptions = subscriptions
        }
