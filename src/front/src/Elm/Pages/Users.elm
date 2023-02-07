module Pages.Users exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form.Input as Input
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), IdType(..), getAuthed, getAuthedQuery, historyUrl, postAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, hardcoded, required)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), allRoles, baseApplication, convertRole, initBase, isAdmin, mapCmd, roleToNumber, rolesDecoder, subscriptionBase, updateBase)
import Main2 exposing (viewBase)
import Multiselect as Multiselect
import NavBar exposing (usersLink)
import UserRolesSelector exposing (userRolesBtns)
import Utils exposing (buildQuery, chunkedView, fillParent, flatten, flex, flex1, flexCenter, largeCentered, mediumMargin, smallMargin, unique)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type alias View =
    { users : WebData (List User)
    , selectedRoles : Multiselect.Model
    , selectedName : Maybe String
    , selectedPhone : Maybe String
    }


type alias User =
    { name : String
    , contact : String
    , roles : List UserRole
    , login : String
    , alteringResult : Maybe (WebData String)
    , id : WebData String
    }



--update


type LocalMsg
    = NoOp
    | GotUsers (Result Http.Error (List User))
    | SelectedRole Multiselect.Msg
    | ChangedName String
    | ChangedPhone String
    | CheckedRole UserRole String
    | GotRemoveRole String (Result Http.Error Bool)
    | GotAddRole String (Result Http.Error Bool)
    | GotIdConversion String (Result Http.Error String) -- username to id


type alias Msg =
    MsgBase LocalMsg


update =
    updateBase updateLocal


updateLocal : LocalMsg -> Model -> ( Model, Cmd Msg )
updateLocal msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model ->
            let
                authed =
                    Authorized auth

                authedSearch =
                    searchForUsers auth.token

                mapRoles roles =
                    case Multiselect.getSelectedValues roles of
                        [] ->
                            Nothing

                        arr ->
                            Just (List.map (\val -> Tuple.first val |> String.toInt |> Maybe.withDefault -1 |> convertRole) arr)

                searchCmd name phone roles =
                    authedSearch name phone (mapRoles roles)

                refreshCmd =
                    authedSearch model.selectedName model.selectedName (mapRoles model.selectedRoles)
            in
            case msg of
                GotUsers (Ok res) ->
                    ( authed <| { model | users = Loaded res }, getUserIds auth.token <| List.map .login res )

                GotUsers (Err err) ->
                    ( authed <| { model | users = Error err }, Cmd.none )

                SelectedRole roleMsg ->
                    let
                        ( subModel, subCmd, _ ) =
                            Multiselect.update roleMsg model.selectedRoles

                        updatedModel =
                            { model | selectedRoles = subModel }

                        search =
                            if Multiselect.getSelectedValues model.selectedRoles /= Multiselect.getSelectedValues updatedModel.selectedRoles then
                                searchCmd updatedModel.selectedName updatedModel.selectedPhone updatedModel.selectedRoles

                            else
                                Cmd.none
                    in
                    ( authed updatedModel, Cmd.batch [ Cmd.map SelectedRole subCmd |> mapCmd, search ] )

                ChangedName name ->
                    ( authed { model | selectedName = Just name }, searchCmd (Just name) model.selectedPhone model.selectedRoles )

                ChangedPhone phone ->
                    ( authed { model | selectedPhone = Just phone }, searchCmd model.selectedName (Just phone) model.selectedRoles )

                CheckedRole role login ->
                    case model.users of
                        Loaded users ->
                            let
                                selectedUser =
                                    Maybe.withDefault (User "" "" [] "" Nothing Loading) <| List.head <| List.filter (\user -> user.login == login) users

                                isRemove =
                                    List.member role selectedUser.roles

                                cmd =
                                    if isRemove then
                                        removeRole auth.token

                                    else
                                        addRole auth.token

                                updateUser user =
                                    if user.login == login then
                                        { user | alteringResult = Just Loading }

                                    else
                                        user
                            in
                            ( authed <| { model | users = Loaded (List.map updateUser users) }, cmd role login )

                        _ ->
                            noOp

                GotAddRole login (Ok _) ->
                    case model.users of
                        Loaded users ->
                            ( m, refreshCmd )

                        _ ->
                            noOp

                GotAddRole login (Err err) ->
                    case model.users of
                        Loaded users ->
                            let
                                updateUser user =
                                    if user.login == login then
                                        { user | alteringResult = Just (Loaded "Failed to add role") }

                                    else
                                        user
                            in
                            ( authed { model | users = Loaded (List.map updateUser users) }, Cmd.none )

                        _ ->
                            noOp

                GotRemoveRole login (Ok _) ->
                    case model.users of
                        Loaded users ->
                            ( m, refreshCmd )

                        _ ->
                            noOp

                GotRemoveRole login (Err _) ->
                    case model.users of
                        Loaded users ->
                            let
                                updateUser user =
                                    if user.login == login then
                                        { user | alteringResult = Just (Loaded "Failed to remove role") }

                                    else
                                        user
                            in
                            ( authed { model | users = Loaded (List.map updateUser users) }, Cmd.none )

                        _ ->
                            noOp

                GotIdConversion login id ->
                    case model.users of
                        Loaded users ->
                            let
                                updateUser user =
                                    if user.login == login then
                                        case id of
                                            Err err ->
                                                { user | id = Error err }

                                            Ok res ->
                                                { user | id = Loaded res }

                                    else
                                        user
                            in
                            ( authed { model | users = Loaded (List.map updateUser users) }, Cmd.none )

                        _ ->
                            noOp

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--view


view : ModelBase View -> Html (MsgBase LocalMsg)
view model =
    viewBase model (Just usersLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    div ([ flex, Flex.col ] ++ fillParent)
        [ div [ flex1, mediumMargin ]
            [ Button.linkButton [ Button.primary, Button.onClick <| Navigate "/user/add", Button.attrs largeCentered ] [ text "Create User" ]
            ]
        , div [ style "flex" "2", flex, Flex.row, Flex.alignItemsCenter ]
            [ viewInput (Input.text [ Input.onInput ChangedName ]) "Name"
            , viewInput (Input.text [ Input.onInput ChangedPhone ]) "Mobile Number"
            , viewInput (Html.map SelectedRole <| Multiselect.view page.selectedRoles) "Roles"
            ]
            |> Html.map Main
        , div [ flex, Flex.row, style "flex" "16", style "overflow-y" "scroll" ]
            [ viewWebdata page.users (chunkedView 3 <| viewUser (isAdmin resp) resp.roles)
            ]
        ]


viewUser : Bool -> List UserRole -> User -> Html Msg
viewUser isAdmin viewerRoles user =
    let
        historyBtn =
            if isAdmin then
                viewWebdata
                    user.id
                    (\id ->
                        Button.linkButton
                            [ Button.outlinePrimary
                            , Button.onClick <| Navigate <| historyUrl "User" id
                            , Button.attrs [ smallMargin ]
                            ]
                            [ text "View history" ]
                    )

            else
                div [] []

        btnViewBase =
            userRolesBtns (\role -> CheckedRole role user.login) user.roles viewerRoles

        btnsMessage msg =
            div [ flex, Flex.col ]
                [ div (largeCentered ++ [ class "text-danger" ]) [ text msg ]
                , btnViewBase
                ]

        btnsView =
            case user.alteringResult of
                Just res ->
                    viewWebdata res btnsMessage

                Nothing ->
                    btnViewBase
    in
    Card.config []
        |> Card.header largeCentered
            [ div largeCentered [ text user.name ]
            ]
        |> Card.block []
            [ Block.titleH4 largeCentered [ text user.login ]
            , Block.titleH4 largeCentered [ text user.contact ]
            , Block.custom <|
                div (flexCenter ++ [ flex ]) [ btnsView |> Html.map Main, historyBtn ]
            ]
        |> Card.view


viewInput : Html msg -> String -> Html msg
viewInput input desc =
    div [ flex, flex1, Flex.col, Flex.alignItemsCenter, smallMargin ]
        [ div largeCentered [ text desc ]
        , input
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialCmd res =
            searchForUsers res.token Nothing Nothing Nothing

        roleToVisible role =
            case role of
                Consumer ->
                    [ ( "1", "Consumer" ) ]

                Producer ->
                    [ ( "1", "Consumer" ), ( "2", "Producer" ) ]

                Manager ->
                    [ ( "1", "Consumer" ), ( "2", "Producer" ), ( "3", "Manager" ) ]

        rolesList =
            case resp of
                Just res ->
                    unique <| flatten <| List.map roleToVisible res.roles

                Nothing ->
                    []

        multiSelect =
            Multiselect.initModel rolesList "roles" Multiselect.Show
    in
    initBase allRoles (View Loading multiSelect Nothing Nothing) initialCmd resp



--cmds


getUserIds : String -> List String -> Cmd Msg
getUserIds token logins =
    let
        expect login =
            Http.expectJson (GotIdConversion login) D.string
    in
    List.map (\login -> getAuthed token (ConvertId <| StringId login) (expect login) Nothing) logins |> Cmd.batch |> mapCmd


removeRole : String -> UserRole -> String -> Cmd Msg
removeRole token role login =
    let
        expect =
            Http.expectJson (GotRemoveRole login) (D.field "success" D.bool)
    in
    postAuthed token (RemoveRole login role) Http.emptyBody expect Nothing |> mapCmd


addRole : String -> UserRole -> String -> Cmd Msg
addRole token role login =
    let
        expect =
            Http.expectJson (GotAddRole login) (D.field "success" D.bool)
    in
    postAuthed token (AddRole login role) Http.emptyBody expect Nothing |> mapCmd


searchForUsers : String -> Maybe String -> Maybe String -> Maybe (List UserRole) -> Cmd Msg
searchForUsers token name contact roles =
    let
        expect =
            Http.expectJson GotUsers usersDecoder

        rolesList =
            List.map (Tuple.mapSecond (List.map roleToNumber)) (justOrEmpty "roles" roles)

        rolesQuery =
            case
                List.head
                    (List.map (\rolePair -> List.map (\role -> ( Tuple.first rolePair, role )) (Tuple.second rolePair)) rolesList)
            of
                Just i ->
                    i

                Nothing ->
                    []

        queryList =
            justOrEmpty "name" name ++ justOrEmpty "phone" contact ++ List.map (Tuple.mapSecond String.fromInt) rolesQuery
    in
    getAuthedQuery (buildQuery queryList) token SearchUsers expect Nothing |> mapCmd


usersDecoder : D.Decoder (List User)
usersDecoder =
    D.field "items" (D.list userDecoder)


userDecoder : D.Decoder User
userDecoder =
    D.succeed User
        |> required "fullName" D.string
        |> required "mobile" D.string
        |> custom (rolesDecoder (D.field "roleCodes" <| D.list D.int))
        |> required "login" D.string
        |> hardcoded Nothing
        |> hardcoded Loading


justOrEmpty : String -> Maybe a -> List ( String, a )
justOrEmpty key val =
    case val of
        Just value ->
            [ ( key, value ) ]

        Nothing ->
            []



--subs


subscriptions : Model -> Sub Msg
subscriptions model =
    subscriptionBase model Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
