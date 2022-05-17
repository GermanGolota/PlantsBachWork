module Pages.Profile exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Utilities.Flex as Flex
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, href, style)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (viewNav)
import Utils exposing (fillParent, flex, flexCenter, largeCentered, mediumMargin, smallMargin)



--model


type alias Model =
    ModelBase View


type alias View =
    { password : String
    }



--update


type Msg
    = NoOp
    | NewPasswordChanged String


update : Msg -> Model -> ( Model, Cmd Msg )
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
                NewPasswordChanged pass ->
                    updateModel { model | password = pass }

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands
--view


view : Model -> Html Msg
view model =
    viewNav model Nothing viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    let
        btnAttr add =
            Button.attrs ([ mediumMargin ] ++ add)
    in
    div ([ flex, Flex.row, mediumMargin ] ++ fillParent ++ flexCenter)
        [ div [ flex, Flex.col, class "modal__container" ]
            [ div largeCentered [ text "New Password" ]
            , Input.password
                [ Input.value page.password
                , Input.onInput NewPasswordChanged
                , Input.attrs
                    [ mediumMargin
                    , style "width" "unset"
                    ]
                ]
            , Button.button [ Button.primary, btnAttr [] ]
                [ text "Change password"
                ]
            , Button.linkButton [ Button.danger, btnAttr [ href "/login/new" ] ] [ text "Logout" ]
            ]
        ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    initBase [ Producer, Consumer, Manager ] (View "") (\res -> Cmd.none) resp


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
