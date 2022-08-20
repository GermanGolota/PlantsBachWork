module UserRolesSelector exposing (userRolesBtns)

import Bootstrap.Button as Button
import Bootstrap.ButtonGroup as ButtonGroup exposing (CheckboxButtonItem)
import Html exposing (Html, text)
import Main exposing (UserRole(..), roleToNumber, roleToStr)
import Utils exposing (fillParent)


userRolesBtns : (UserRole -> msg) -> List UserRole -> List UserRole -> Html msg
userRolesBtns clickEvent userRoles viewerRoles =
    let
        maxViewer =
            Maybe.withDefault -1 <| List.maximum <| List.map roleToNumber viewerRoles

        canEdit role =
            roleToNumber role <= maxViewer

        roles =
            [ Consumer, Producer, Manager ]
    in
    ButtonGroup.checkboxButtonGroup [ ButtonGroup.attrs fillParent ]
        (List.map
            (\role -> btnView clickEvent (List.member role userRoles) role (canEdit role))
            roles
        )


btnView : (UserRole -> msg) -> Bool -> UserRole -> Bool -> CheckboxButtonItem msg
btnView clickEvent checked role canCheck =
    ButtonGroup.checkboxButton
        checked
        [ Button.primary, Button.onClick <| clickEvent role, Button.disabled <| not canCheck ]
        [ text <| roleToStr role ]
