﻿(* Copyright 1999-2005 The Apache Software Foundation or its licensors, as
 * applicable.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *)

namespace FSharp.Client.Formlet.WPF

open System
open System.Windows
open System.Windows.Controls

open FSharp.Client.Formlet.Core

open Controls

module Input =

    let Text initialText : Formlet<FormletContext, UIElement, string> =
        let eval (fc,ft : FormletTree<UIElement>) =
            match ft with
            | Singleton (:? InputTextElement as e)  -> (FormletCollect.Success e.Text)     , ft
            | _                                     -> (FormletCollect.Success initialText), Singleton (upcast new InputTextElement(initialText))

        Formlet.New eval

    let Integer v =
        let map (collect : FormletCollect<string>) : FormletCollect<int> =
            if collect.Failures.Length > 0 then
                FormletCollect.New 0 collect.Failures
            else
                let mutable i = 0
                if Int32.TryParse (collect.Value, &i) then
                    FormletCollect.Success i
                else
                    FormletCollect<_>.FailWith "Input is not an integer"
        Text (v.ToString())
        |> Formlet.MapResult map

module Enhance =

(*
    let Many (initialCount : int) (f : Formlet<UIElement, 'T>) : Formlet<UIElement, 'T list> =
        let eval (fc,ft : FormTree<UIElement>) =
            match ft with
            | Container ((:? ListBox as lb), fts : FormTree<UIElement> list) ->
                let ifts =
                (Formlet.Success []) , ft
            | _                         ->
                (Formlet.Success []) , Visual (upcast new ListBox ())

        Formlet.New eval

*)


    let WithErrorVisual (f : Formlet<FormletContext, UIElement, 'T>) : Formlet<FormletContext, UIElement, 'T> =
        let eval (fc,ft : FormletTree<UIElement>) =
            let ift =
                match ft with
                | Apply (_,ft)  -> ft
                | _             -> Empty
            let c,nift  = f.Evaluate (fc, ift)
            let apply   = if c.Failures.IsEmpty then RemoveErrorAdorner else AppendErrorAdorner
            c, Apply (apply, nift)

        Formlet.New eval