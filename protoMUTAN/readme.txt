protoMUTAN aims to define and develop a simple to use set of syntax that will act as the control language to be used on AZUSA. As the name suggests, protoMUTAN is a prototype and is subject to possibly intensive modification as long as AZUSA kernel is yet to be completed. 

Once the set of syntax has been well tested and its practicality verified. It will be renamed to MUTAN.

The goal of this side project is to develop an interpreter that can understand protoMUTAN.

Semiformal definition of protoMUTAN:

expr  :=  *          (cannot be emtpy, a string, a logical or an arithmetic expression, e.g. 1+1, (1>2)&(VAR=3), ~(true&true|false),etc.)
decla :=  [$]ID=expr (a declaration statement for a variable, e.g. VAR=NYAN , X=2 ,etc.)
exec  :=  RID(expr|\lambda)   (an execution command that asks AZUSA to perform a function, e.g. IMG:nyan.png , SAY:{name})
basic  :=  decla|exec
multi :=  basic{;basic}
cond  :=  expr?multi
stmt  :=  basic|multi|cond
stmts :=  stmt{;stmt}
loop  :=  @stmts+ 
line  :=  stmts|loop

(block definition)
namedblock :=  
.ID{
block
{block}
}

condblock  :=  
expr{
block
{block}
}

loopblock :=
@{
block
{block}
}

block := line|namedblock|condblock|loopblock


program := 
block
{block}


