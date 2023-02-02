port module Pages.Login exposing (main)

import Assets exposing (treeIcon)
import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form as Form
import Bootstrap.Form.Input as Input
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Color exposing (Color, toCssString)
import Dict
import Endpoints exposing (Endpoint(..), endpointToUrl)
import Html exposing (Html, div, text)
import Html.Attributes exposing (for, style)
import Http as Http
import Json.Decode as D
import Json.Encode as E
import Main exposing (AuthResponse, MsgBase(..), UserRole(..), baseApplication, mapCmd, roleToStr, rolesDecoder, updateBase)
import TypedSvg.Types exposing (px)
import Utils exposing (fillParent, filledBackground, flexCenter, mapStyles, rgba255, textCenter)
import Webdata exposing (WebData(..), viewWebdata)


submitSuccessDecoder : D.Decoder AuthResponse
submitSuccessDecoder =
    D.map3 AuthResponse
        (D.field "token" D.string)
        (rolesDecoder (D.field "roles" (D.list D.int)))
        (D.field "username" D.string)


encodeResponse : AuthResponse -> E.Value
encodeResponse response =
    E.object [ ( "token", E.string response.token ), ( "roles", E.list roleToValue response.roles ), ( "username", E.string response.username ) ]


roleToValue : UserRole -> E.Value
roleToValue role =
    E.string <| roleToStr role


port notifyLoggedIn : E.Value -> Cmd msg


credsEncoded : Model -> E.Value
credsEncoded model =
    let
        list =
            [ ( "login", E.string model.username )
            , ( "password", E.string model.password )
            ]
    in
    list
        |> E.object


submit : Model -> Cmd Msg
submit model =
    let
        body =
            credsEncoded model |> Http.jsonBody
    in
    Http.post
        { url = endpointToUrl Login
        , body = body
        , expect = Http.expectJson SubmitRequest submitSuccessDecoder
        }
        |> mapCmd


type CredsStatus
    = BadCredentials
    | GoodCredentials


type alias Model =
    { username : String
    , password : String
    , status : Maybe (WebData CredsStatus)
    }


type LocalMsg
    = UsernameUpdated String
    | PasswordUpdate String
    | Submitted
    | SubmitRequest (Result Http.Error AuthResponse)


type alias Msg =
    MsgBase LocalMsg



-- update


update : Msg -> Model -> ( Model, Cmd Msg )
update =
    updateBase updateLocal


updateLocal : LocalMsg -> Model -> ( Model, Cmd Msg )
updateLocal msg model =
    case msg of
        UsernameUpdated login ->
            ( { model | username = login, status = Nothing }, Cmd.none )

        PasswordUpdate pass ->
            ( { model | password = pass, status = Nothing }, Cmd.none )

        Submitted ->
            ( { model | status = Just Loading }, submit model )

        SubmitRequest (Ok response) ->
            ( { model | status = Just <| Loaded GoodCredentials }, notifyLoggedIn <| encodeResponse response )

        SubmitRequest (Err err) ->
            ( { model | status = Just <| Loaded BadCredentials }, Cmd.none )


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init _ _ =
    ( Model "" "" Nothing, Cmd.none )


greenColor : Float -> Color
greenColor opacity =
    rgba255 36 158 71 opacity


view : Model -> Html Msg
view model =
    viewLocal model |> Html.map Main


viewLocal : Model -> Html LocalMsg
viewLocal model =
    Grid.containerFluid [ style "height" "100vh" ]
        [ Grid.row [ Row.attrs (fillParent ++ flexCenter) ]
            [ Grid.col [] []
            , Grid.col [] []
            , Grid.col [ Col.middleXs ]
                [ viewForm model
                , viewBackground
                ]
            , Grid.col [] []
            , Grid.col [] []
            ]
        ]


viewBackground =
    filledBackground <|
        mapStyles <|
            Dict.fromList
                [ ( "background"
                  , "linear-gradient(180deg, #C4C4C4 0%, #159A42 0.01%, rgba(0, 128, 0, 0.53) 53.65%, #006400 100%)"
                  )
                , ( "box-shadow"
                  , "0px 4px 4px rgba(0, 0, 0, 0.25)"
                  )
                ]


viewForm : Model -> Html LocalMsg
viewForm model =
    Card.config [ Card.attrs [ style "width" "100%", style "opacity" "0.66" ] ]
        |> Card.header [ textCenter ]
            [ treeIcon (px 200) (greenColor 1)
            ]
        |> Card.block []
            [ Block.custom <| viewFormMain model
            ]
        |> Card.view


viewFormMain : Model -> Html LocalMsg
viewFormMain model =
    let
        updatePass pass =
            PasswordUpdate pass

        updateLogin login =
            UsernameUpdated login

        credView tuple =
            Form.group [ Form.attrs [ style "color" <| toCssString <| Tuple.first tuple ] ]
                [ text <| Tuple.second tuple ]

        credDisplay =
            case model.status of
                Just status ->
                    viewWebdata status (\cred -> credView <| displayFromCredStatus cred)

                Nothing ->
                    div [] []

        btnDisabled =
            case model.status of
                Just _ ->
                    True

                Nothing ->
                    False
    in
    div []
        [ credDisplay
        , Form.form
            []
            [ Form.group []
                [ Form.label [ for "login" ] [ text "Login" ]
                , Input.text
                    [ Input.id "login"
                    , Input.onInput updateLogin
                    ]
                ]
            , Form.group []
                [ Form.label [ for "password" ] [ text "Password" ]
                , Input.password
                    [ Input.id "password"
                    , Input.onInput updatePass
                    ]
                ]
            ]
        , Form.group []
            [ Button.button
                [ Button.primary
                , Button.onClick Submitted
                , Button.disabled btnDisabled
                , Button.attrs
                    [ style "margin" "10px auto"
                    , style "width" "100%"
                    ]
                ]
                [ text "Login" ]
            ]
        ]


displayFromCredStatus : CredsStatus -> ( Color, String )
displayFromCredStatus status =
    case status of
        BadCredentials ->
            ( Color.red, "Those credentials are not valid!" )

        GoodCredentials ->
            ( Color.green, "Those credentials are valid! You would be redirected shortly" )


subscriptions : Model -> Sub Msg
subscriptions _ =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
