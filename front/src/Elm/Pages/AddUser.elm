module Pages.AddUser exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style, value)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (usersLink, viewNav)
import UserRolesSelector exposing (userRolesBtns)
import Utils exposing (fillParent, flex, flexCenter, largeCentered, mediumMargin)



--model


type alias Model =
    ModelBase View


languages : List String
languages =
    [ "English", "Español", "Українська" ]


type alias View =
    { firstName : String
    , lastName : String
    , login : String
    , phone : String
    , email : String
    , selectedLanguage : String
    , selectedRoles : List UserRole
    }



--update


type Msg
    = NoOp
    | FirstNameChanged String
    | LastNameChanged String
    | EmailChanged String
    | LoginChanged String
    | PhoneChanged String
    | LanguageSelected String
    | RoleSelected UserRole
    | Submit


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
                FirstNameChanged name ->
                    updateModel { model | firstName = name }

                LastNameChanged name ->
                    updateModel { model | lastName = name }

                EmailChanged email ->
                    updateModel { model | email = email }

                LoginChanged login ->
                    updateModel { model | login = login }

                PhoneChanged phone ->
                    updateModel { model | phone = phone }

                LanguageSelected lang ->
                    updateModel { model | selectedLanguage = lang }

                RoleSelected role ->
                    let
                        newRoles =
                            if List.member role model.selectedRoles then
                                List.filter (\r -> r /= role) model.selectedRoles

                            else
                                model.selectedRoles ++ [ role ]
                    in
                    updateModel { model | selectedRoles = newRoles }

                Submit ->
                    noOp

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands
--view


view : Model -> Html Msg
view model =
    viewNav model (Just usersLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    div ([ flex, Flex.row ] ++ fillParent ++ flexCenter)
        [ div [ flex, Flex.col, class "modal__container" ]
            [ div [ flex, Flex.row ]
                (viewInputs page)
            , div [ flex, Flex.row, mediumMargin ]
                [ userRolesBtns RoleSelected page.selectedRoles resp.roles ]
            , div [ flex, Flex.row, Flex.justifyEnd, mediumMargin ]
                [ viewBtn ]
            ]
        ]


viewInputs : View -> List (Html Msg)
viewInputs page =
    [ div [ flex, Flex.col, mediumMargin ]
        (viewInput "First Name" page.firstName FirstNameChanged
            ++ viewInput "Last Name" page.lastName LastNameChanged
            ++ viewInput "Email" page.email EmailChanged
        )
    , div [ flex, Flex.col, mediumMargin ]
        (viewInput "Login" page.login LoginChanged ++ viewInput "Phone Number" page.phone PhoneChanged ++ viewInputBase "Invite Language" languagesSelector)
    ]


languagesSelector : Html Msg
languagesSelector =
    Select.select [ Select.onChange LanguageSelected ] <| List.map (\lang -> Select.item [ value lang ] [ text lang ]) languages


viewInput : String -> String -> (String -> msg) -> List (Html msg)
viewInput desc val change =
    viewInputBase desc <| Input.text [ Input.value val, Input.onInput change ]


viewInputBase : String -> Html msg -> List (Html msg)
viewInputBase desc input =
    [ div largeCentered [ text desc ], input ]


viewBtn =
    Button.button [ Button.primary, Button.onClick Submit ] [ text "Create and invite" ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialLang =
            Maybe.withDefault "English" <| List.head languages
    in
    initBase [ Producer, Consumer, Manager ] (View "" "" "" "" "" initialLang []) (\res -> Cmd.none) resp


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
