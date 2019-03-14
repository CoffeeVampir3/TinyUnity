# TinyUnity
A C# implementation of the ANTLR4 Tiny syntax by bkiers. Lists still a WIP.  
[Bkiers Tiny Language](https://github.com/bkiers/tiny-language-antlr4)

# Antlr4
See the installation guide for VS here. Currently doesnt play well with unity, working on it.  
[Antlr4 for VS](https://github.com/tunnelvisionlabs/antlr4cs)

# How To (standalone project):
1: Install Antlr4 C# packages and language support for VS as above.  
2: Add TinyUnity.g4 as a link to your project using "Add Existing Item" and changing "Add" to "Add as link"  
3: Build project  

# How to (Unity)
1: Suffer

# Assertions
Currently passes the folowing assertions:
```
// boolean expressions
assert(true || false);
assert(!false);
assert(true && true);
assert(!true || !false);
assert(true && (true || false));

// relational expressions
assert(1 < 2);
assert(666 >= 666);
assert(-5 > -6);
assert(0 >= -1);
assert('a' < 's');
assert('sw' <= 'sw');

// add
assert(1 + 999 == 1000);
assert(2 - -2 == 4);
assert(-1 + 1 == 0);
assert(1 - 50 == -49);

// multiply
assert(3 * 50 == 150);
assert(4 / 2 == 2);
assert(1 / 4 == 0.25);
assert(999999 % 3 == 0);
assert(-5 * -5 == 25);

// power
assert(2^10 == 1024);
assert(3^3 == 27);
assert(4^3^2 == 262144); // power is right associative
assert((4^3)^2 == 4096);

// for- and while statements
a = 0;
for(i = 1; 10) {
 a = a + i;
}
assert(a == (1+2+3+4+5+6+7+8+9+10));

b = -10;
c = 0;
while(b < 0) { 
  c = c + b;
  b = b + 1;
}
assert(c == -(1+2+3+4+5+6+7+8+9+10));

// if
a = 123;
if( a > 200 ) {
  assert(false);
}

if( a < 100 ) {
	assert(false);
} else if (a > 125) {
	assert(false);
} else if (a < 124) {
	assert(true);
} else {
	assert(false);
}

if(false){
  assert(false);
} else {
  assert(true);
}

// functions
function twice(n) {
  temp = n + n; 
  return temp; 
}

function squared(n) {
  return n*n; 
}

function squaredAndTwice(n) {
  return twice(squared(n)); 
}

assert(squared(666) == 666^2);
assert(twice(squared(5)) == 50);
assert(squaredAndTwice(10) == 200);
assert(squared(squared(squared(2))) == ((2^2)^2)^2);

println('All assertions passed.');
```
