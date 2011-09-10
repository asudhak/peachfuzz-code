// Thomas Nagy, 2011

#include "foo.h"

Foo::Foo() : QWidget(NULL) {

}

#if WAF
#include "foo.moc"
#endif
