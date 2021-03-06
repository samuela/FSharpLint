﻿module TestNestedStatements

open NUnit.Framework
open FSharpLint.Rules.NestedStatements
open FSharpLint.Framework.Configuration

let config = 
    Map.ofList 
        [ 
            (AnalyserName, 
                { 
                    Rules = Map.ofList []
                    Settings = Map.ofList 
                        [ 
                            ("Enabled", Enabled(true)) 
                            ("Depth", Depth(5)) 
                        ]
                })
            ]
 
[<TestFixture>]
type TestNestedStatements() =
    inherit TestRuleBase.TestRuleBase(analyser, config)

    [<Category("Performance")>]
    [<Test>]
    member this.``Performance of nested statements analyser``() = 
        Assert.Less(this.TimeAnalyser(100, defaultConfiguration), 20)

    [<Test>]
    member this.NestedTooDeep() = 
        this.Parse """
module Program

let dog =
    if true then
        if true then
            if true then
                if true then
                    if true then
                        if true then
                            ()
    ()"""

        Assert.IsTrue(this.ErrorExistsAt(9, 20)) 

    [<Test>]
    member this.``Repeated nested too deep complains for correct ranges``() = 
        this.Parse """
module Program

let dog =
    if true then
        if true then
            if true then
                if true then
                    if true then
                        if true then
                            ()
    if true then
        if true then
            if true then
                if true then
                    if true then
                        if true then
                            ()
    ()"""
    
        Assert.IsTrue(this.ErrorExistsAt(9, 20)) 
        Assert.IsTrue(this.ErrorExistsAt(16, 20)) 

    [<Test>]
    member this.NestedTooDeepSuppressed() = 
        this.Parse """
module Program

[<System.Diagnostics.CodeAnalysis.SuppressMessage("NestedStatements", "*")>]
let dog =
    if true then
        if true then
            if true then
                if true then
                    if true then
                        if true then
                            ()
    ()"""

        Assert.IsFalse(this.ErrorExistsOnLine(10)) 

    [<Test>]
    member this.ElseIfsShouldNotCountAsNested() = 
        this.Parse """
module Program

let dog =
    if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else if true then
        ()
    else 
        ()"""

        Assert.IsFalse(this.ErrorExistsAt(13, 4)) 
        
    [<Test>]
    member this.LambdaWildcardArgumentsMustNotCountAsANestedStatement() = 
        this.Parse """
module Program

let dog = (fun _ _ _ _ _ _ _ _ -> ())"""

        Assert.IsFalse(this.ErrorExistsOnLine(4))

    [<Test>]
    member this.LambdaArgumentsMustNotCountAsANestedStatement() = 
        this.Parse """
module Program

let dog = (fun a b c d e f g h i j -> ())"""

        Assert.IsFalse(this.ErrorExistsOnLine(4))

    [<Test>]
    member this.NestedLambdasCountedCorrectly() = 
        this.Parse """
module Program

let dog = (fun x -> fun x -> fun x -> fun x -> fun x -> ())"""

        Assert.IsTrue(this.ErrorExistsAt(4, 47))