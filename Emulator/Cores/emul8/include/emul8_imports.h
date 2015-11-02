#ifndef EMUL8_IMPORTS_H_
#define EMUL8_IMPORTS_H_

#include "emul8_imports_generated.h"

#define emul8_direct_glue(a, b) a##b

#define emul8_glue(a, b) emul8_direct_glue(a, b)

#define emul8_func_header(TYPE, IMPORTED_NAME, LOCAL_NAME) \
void emul8_glue(emul8_glue(emul8_glue(emul8_external_attach__, emul8_glue(EMUL8_EXT_TYPE_, TYPE)), __), IMPORTED_NAME) (TYPE param)

#define EXTERNAL(TYPE, NAME) EXTERNAL_AS(TYPE, $##NAME, NAME)

#define EXTERNAL_AS(TYPE, IMPORTED_NAME, LOCAL_NAME) \
    TYPE emul8_glue(LOCAL_NAME, _callback$);\
    emul8_glue(TYPE, _return$) LOCAL_NAME (emul8_glue(TYPE, _args$)) \
    {\
       emul8_glue(TYPE, _keyword$) (* emul8_glue(LOCAL_NAME, _callback$)) (emul8_glue(TYPE, _vars$)); \
    }\
    emul8_func_header(TYPE, IMPORTED_NAME, LOCAL_NAME)\
    {\
      emul8_glue(LOCAL_NAME, _callback$) = param;\
    }


#endif
