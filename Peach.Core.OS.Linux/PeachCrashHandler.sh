#!/bin/sh

SCRIPT=$(readlink -f $0)
PATTERN="|${SCRIPT} -p=%p -u=%u -g=%g -s=%s -t=%t -h=%h -e=%e -E=%E"

if [ "$#" -eq "0" ];
then
	echo "Peach 3 Linux Crash Handler"
	echo ""
	echo "Installation:"
	echo "echo \"$PATTERN\" > /proc/sys/kernel/core_pattern"
	echo ""
	exit 0
fi

UID=""
GID=""
SIG=""
TIME=""
HOST=""
EXE=""
DIR=""
PID=""
LOG="/var/peachcrashes"

while :
do
	case $1 in
		-u=*)
			UID=${1#*=}
			shift
			;;
		-g=*)
			GID=${1#*=}
			shift
			;;
		-s=*)
			SIG=${1#*=}
			shift
			;;
		-t=*)
			TIME=${1#*=}
			shift
			;;
		-h=*)
			HOST=${1#*=}
			shift
			;;
		-e=*)
			EXE=${1#*=}
			shift
			;;
		-E=*)
			DIR=${1#*=}
			shift
			;;
		-p=*)
			PID=${1#*=}
			shift
			;;
		-l=*)
			LOG=${1#*=}
			shift
			;;
		*)
			break
			;;
	esac
done

if [ -z "${LOG}" ];
then
	LOG="."
fi

FILE="${LOG}/peach_${EXE}_${PID}"
OUTDIR=$(dirname ${FILE})
INFO="${FILE}.info"
CORE="${FILE}.core"
TEMP=$(mktemp)

# Make the date human readable
PRETTY_TIME=$(date --date="@${TIME}") || $TIME

# Get full path to program, replacing ! with /

PROG="${DIR}/${EXE}"

while :
do
	LEFT=${PROG%%\!*}
	RIGHT=${PROG#*\!}

	if [ "$RIGHT" = "${PROG}" ];
	then
		break
	else
		PROG="${LEFT}/${RIGHT}"
	fi
done

echo "Temp: ${TEMP}"
echo "Dir : ${OUTDIR}"
echo "Core: ${CORE}"
echo "Info: ${INFO}"

# Ensure destination directory exists
mkdir -p ${OUTDIR} || exit $?

# Write all of stdin to CORE
cat - > ${CORE}

cat >> ${TEMP} << EOF
Linux Crash Handler -- Crash Information
========================================

PID: ${PID}
EXE: ${PROG}
UID: ${UID}
GID: ${GID}
SIG: ${SIG}

Host: ${HOST}
Date: ${PRETTY_TIME}

GDB Output
----------
EOF

GDB="$(which gdb)"

if [ -z $GDB ];
then
	echo "Error: gdb could not be found." >> ${TEMP}
else
	echo "${GDB} ${PROG} ${CORE}\n" >> ${TEMP}

$GDB \
--batch \
--nx \
--quiet \
-ex "echo \n--- Info Frame ---\n\n" \
-ex "info frame" \
-ex "echo \n--- Info Registers ---\n\n" \
-ex "info registers" \
-ex "echo \n--- Backtrace ---\n\n" \
-ex "thread apply all bt full" \
-ex "quit" \
${PROG} \
${CORE} 1>>${TEMP} 2>&1

fi

# Move .info into place last since this triggers the monitor
mv ${TEMP} ${INFO} || exit $?

echo "Done!"

