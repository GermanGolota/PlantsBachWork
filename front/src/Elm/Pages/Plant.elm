module Pages.Plant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Radio as Radio
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Dict
import Endpoints exposing (Endpoint(..), getAuthed, imageIdToUrl)
import Html exposing (Html, div, i, text)
import Html.Attributes exposing (class, href, style, value)
import Http
import ImageList as ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import NavBar exposing (searchLink, viewNav)
import Utils exposing (fillParent, flex, flex1, formatPrice, largeCentered, largeFont, mediumFont, mediumMargin, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant PlantView


type alias PlantView =
    { id : Int
    , plantType : ViewType
    }


type ViewType
    = JustPlant (WebData (Maybe PlantModel))
    | Order (WebData (Maybe PlantModel)) (WebData (List DeliveryAddress)) SelectedAddress


type SelectedAddress
    = None
    | City String
    | Location Int
    | Selected DeliveryAddress


type alias DeliveryAddress =
    { city : String
    , location : Int
    }



--update


type alias PlantModel =
    { name : String
    , description : String
    , price : Float
    , soil : String
    , regions : List String
    , group : String

    --{createdDate}({createdHumanDate})
    , created : String
    , sellerName : String
    , sellerPhone : String
    , sellerCreds : PersonCreds
    , caretakerCreds : PersonCreds
    , images : ImageList.Model
    }


type alias PersonCreds =
    { sold : Int
    , cared : Int
    , instructions : Int
    }


type Msg
    = GotPlant (Result Http.Error (Maybe PlantModel))
    | Images ImageList.Msg
    | GotAddresses (Result Http.Error (List DeliveryAddress))
    | AddressSelected String Int
    | CityChanged String
    | LocationChanged Int


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    case m of
        Authorized auth model ->
            let
                authed newModel =
                    Authorized auth newModel

                authedPlant plantView =
                    authed <| Plant plantView
            in
            case ( msg, model ) of
                ( GotPlant (Ok res), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Loaded res }, Cmd.none )

                        Order pWeb del selected ->
                            ( authedPlant <| { p | plantType = Order (Loaded res) del selected }, Cmd.none )

                ( GotPlant (Err res), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Error }, Cmd.none )

                        Order pWeb del selected ->
                            ( authedPlant <| { p | plantType = Order Error del selected }, Cmd.none )

                ( GotAddresses (Ok res), Plant p ) ->
                    let
                        sel =
                            case List.head res of
                                Just val ->
                                    Selected val

                                Nothing ->
                                    None
                    in
                    case p.plantType of
                        Order pl del selected ->
                            ( authedPlant <| { p | plantType = Order pl (Loaded res) sel }, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( GotAddresses (Err res), Plant p ) ->
                    case p.plantType of
                        Order pl del selected ->
                            ( authedPlant <| { p | plantType = Order pl Error selected }, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( Images img, Plant p ) ->
                    case p.plantType of
                        JustPlant (Loaded (Just pl)) ->
                            ( authedPlant { p | plantType = JustPlant <| Loaded <| Just { pl | images = ImageList.update img pl.images } }, Cmd.none )

                        Order (Loaded (Just pl)) del selected ->
                            ( authedPlant { p | plantType = Order (Loaded <| Just { pl | images = ImageList.update img pl.images }) del selected }, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( _, _ ) ->
                    ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands


getAddressesCommand : String -> Cmd Msg
getAddressesCommand token =
    let
        expect =
            Http.expectJson GotAddresses addressesDecoder
    in
    getAuthed token Addresses expect Nothing


addressesDecoder : D.Decoder (List DeliveryAddress)
addressesDecoder =
    D.field "addresses" (D.list addressDecoder)


addressDecoder : D.Decoder DeliveryAddress
addressDecoder =
    D.succeed DeliveryAddress
        |> required "city" D.string
        |> required "mailNumber" D.int


getPlantCommand : String -> Int -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder token)
    in
    getAuthed token (PlantE plantId) expect Nothing


plantDecoder : String -> D.Decoder (Maybe PlantModel)
plantDecoder token =
    let
        initedDecoder =
            plantItemDecoder token
    in
    D.field "exists" D.bool |> D.andThen initedDecoder


plantItemDecoder : String -> Bool -> D.Decoder (Maybe PlantModel)
plantItemDecoder token exists =
    if exists then
        D.map Just (plantDecoderBase token)

    else
        D.succeed Nothing


plantDecoderBase : String -> D.Decoder PlantModel
plantDecoderBase token =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed PlantModel
        |> requiredItem "plantName" D.string
        |> requiredItem "description" D.string
        |> requiredItem "price" D.float
        |> requiredItem "soilName" D.string
        |> requiredItem "regions" (D.list D.string)
        |> requiredItem "groupName" D.string
        |> custom createdDecoder
        |> requiredItem "sellerName" D.string
        |> requiredItem "sellerPhone" D.string
        |> custom (credsDecoder "seller")
        |> custom (credsDecoder "careTaker")
        |> custom (imagesDecoder token)


imagesDecoder : String -> D.Decoder ImageList.Model
imagesDecoder token =
    let
        baseDecoder =
            imageIdsToModel token
    in
    D.map baseDecoder (D.at [ "item", "images" ] (D.list D.int))


imageIdsToModel : String -> List Int -> ImageList.Model
imageIdsToModel token ids =
    let
        baseList =
            List.map (\id -> ( id, imageIdToUrl token id )) ids
    in
    ImageList.fromDict <| Dict.fromList baseList


credsDecoder : String -> D.Decoder PersonCreds
credsDecoder person =
    let
        combine str =
            person ++ str

        requiredItem val =
            requiredAt [ "item", val ]
    in
    D.succeed PersonCreds
        |> requiredItem (combine "Cared") D.int
        |> requiredItem (combine "Sold") D.int
        |> requiredItem (combine "Instructions") D.int


createdDecoder : D.Decoder String
createdDecoder =
    let
        combine date humanDate =
            date ++ "(" ++ humanDate ++ ")"
    in
    D.map2
        combine
        (D.at [ "item", "createdDate" ] D.string)
        (D.at [ "item", "createdHumanDate" ] D.string)



--view


view : Model -> Html Msg
view model =
    viewNav model (Just searchLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage _ page =
    case page of
        NoPlant ->
            div [] [ text "Please select a plant", Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href "/search" ] ] [ text "Return to search" ] ]

        Plant plant ->
            let
                plantView =
                    viewPlantFull plant.id

                orderView =
                    viewOrderFull plant.id
            in
            case plant.plantType of
                JustPlant pl ->
                    viewWebdata pl (plantView viewPlant)

                Order pl del selected ->
                    viewWebdata pl (orderView selected del)


viewOrderFull : Int -> SelectedAddress -> WebData (List DeliveryAddress) -> Maybe PlantModel -> Html Msg
viewOrderFull id selected del p =
    viewPlantFull id (viewOrder selected del) p


viewOrder : SelectedAddress -> WebData (List DeliveryAddress) -> Int -> PlantModel -> Html Msg
viewOrder selected del id pl =
    let
        header textT =
            div largeCentered [ text textT ]
    in
    div (fillParent ++ [ flex, Flex.row ])
        [ viewPlantLeft pl
        , div [ flex, Flex.col, flex1 ]
            (viewDesc pl
                ++ [ header "Payment methods"
                   , div [ flex1, Flex.justifyCenter, flex, Flex.col ]
                        [ Radio.advancedCustom
                            [ Radio.id "direct"
                            , Radio.disabled True
                            ]
                            (Radio.label [] [ text "Pay Now" ])
                        , Radio.advancedCustom
                            [ Radio.id "arrival"
                            , Radio.checked True
                            ]
                            (Radio.label [] [ text "Pay On Arrival" ])
                        ]
                   , viewWebdata del (viewLocation selected)
                   , interactionButtons True id
                   ]
            )
        ]


viewLocation : SelectedAddress -> List DeliveryAddress -> Html Msg
viewLocation sel dels =
    let
        viewDel delAddr =
            Select.item [ value (delAddr.city ++ "_" ++ String.fromInt delAddr.location) ] [ text (delAddr.city ++ " | " ++ locationToString delAddr.location) ]
    in
    div (fillParent ++ [ flex, Flex.col, mediumMargin, flex1 ])
        ([ div largeCentered [ text "Previous" ]
         , Select.select []
            (List.map viewDel dels)
         ]
            ++ viewSelected sel
        )


locationToString : Int -> String
locationToString location =
    String.fromInt location ++ ", Nova Poshta Delivery Address"


viewSelected : SelectedAddress -> List (Html Msg)
viewSelected selected =
    let
        cityText =
            case selected of
                Selected addr ->
                    addr.city

                City city ->
                    city

                _ ->
                    ""

        locationText =
            case selected of
                Selected addr ->
                    locationToString addr.location

                Location location ->
                    locationToString location

                _ ->
                    ""
    in
    [ div largeCentered [ text "City" ]
    , div [ flex, flex1, Flex.row, Flex.alignItemsCenter ]
        [ i [ class "fa-solid fa-location-dot", smallMargin ] []
        , Input.text [ Input.value cityText ]
        ]
    , div largeCentered [ text "Location" ]
    , Input.text [ Input.attrs [ flex1 ], Input.value locationText ]
    ]


viewPlantFull : Int -> (Int -> PlantModel -> Html Msg) -> Maybe PlantModel -> Html Msg
viewPlantFull id viewFunc p =
    case p of
        Just plant ->
            viewFunc id plant

        Nothing ->
            div [] [ text "This plant is no longer available, sorry" ]


viewPlant : Int -> PlantModel -> Html Msg
viewPlant id plant =
    let
        filled args =
            fillParent ++ args
    in
    div (filled [ flex, Flex.row ])
        [ viewPlantLeft plant
        , div [ flex, Flex.col, flex1 ]
            (viewDesc plant
                ++ [ viewPlantStat "Soil" plant.soil
                   , viewPlantStat "Regions" (String.join ", " plant.regions)
                   , viewPlantStat "Group" plant.group
                   , viewPlantStat "Age" plant.created
                   , interactionButtons False id
                   ]
            )
        ]


viewDesc : PlantModel -> List (Html msg)
viewDesc plant =
    [ div largeCentered [ text "Description" ]
    , div [] [ text plant.description ]
    , div largeCentered [ text <| formatPrice plant.price ]
    ]


viewPlantLeft : PlantModel -> Html Msg
viewPlantLeft plant =
    div [ flex, Flex.col, flex1 ]
        [ div largeCentered [ text plant.name ]
        , Html.map Images <| ImageList.view plant.images
        , viewPlantStat "Caretaker Credentials" (credsToString plant.caretakerCreds)
        , viewPlantStat "Seller Credentials" (credsToString plant.sellerCreds)
        , viewPlantStat "Seller Nama" plant.sellerName
        , viewPlantStat "Seller Phone" plant.sellerPhone
        ]


credsToString : PersonCreds -> String
credsToString creds =
    String.fromInt creds.cared ++ " cared, " ++ String.fromInt creds.sold ++ " sold, " ++ String.fromInt creds.instructions ++ " instructions published"


viewPlantStat desc value =
    flexRowGap (div [ mediumMargin, mediumFont ] [ text desc ]) (div [ mediumMargin, mediumFont ] [ text value ])


flexRowGap left right =
    div [ flex, Flex.row, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
        [ left
        , right
        ]


interactionButtons : Bool -> Int -> Html Msg
interactionButtons isOrder id =
    let
        backUrl =
            if isOrder then
                "/plant/" ++ String.fromInt id

            else
                "/search"

        orderText =
            if isOrder then
                "Confirm Order"

            else
                "Order"

        orderUrl =
            if isOrder then
                "#"

            else
                "/plant/" ++ String.fromInt id ++ "/order"
    in
    div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
        [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href backUrl, largeFont ] ] [ text "Back" ]
        , Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href orderUrl, largeFont ] ] [ text orderText ]
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialModel =
            decodeInitial flags

        cmds authResp =
            case initialModel of
                Plant p ->
                    case p.plantType of
                        JustPlant _ ->
                            getPlantCommand authResp.token p.id

                        Order _ _ _ ->
                            Cmd.batch [ getPlantCommand authResp.token p.id, getAddressesCommand authResp.token ]

                NoPlant ->
                    Cmd.none
    in
    initBase
        [ Producer, Consumer, Manager ]
        initialModel
        cmds
        resp


decodeInitial : D.Value -> View
decodeInitial flags =
    let
        isOrder =
            decodeIsOrder flags
    in
    case decodePlantId flags of
        Err _ ->
            NoPlant

        Ok plantId ->
            case String.toInt plantId of
                Just plantNumber ->
                    if isOrder then
                        Plant (PlantView plantNumber <| Order Loading Loading None)

                    else
                        Plant (PlantView plantNumber <| JustPlant Loading)

                Nothing ->
                    NoPlant


decodeIsOrder : D.Value -> Bool
decodeIsOrder flags =
    let
        decoded =
            D.decodeValue (D.field "isOrder" D.bool) flags
    in
    case decoded of
        Ok res ->
            res

        Err _ ->
            False


decodePlantId : D.Value -> Result D.Error String
decodePlantId flags =
    D.decodeValue (D.field "plantId" D.string) flags


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
