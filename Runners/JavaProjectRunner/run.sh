git clone https://${GITHUB_OAUTH_TOKEN}:x-oauth-basic@github.com/${GITHUB_ORG_NAME}/${GITHUB_SUBMISSION_REPO_NAME} Submission
git clone https://${GITHUB_OAUTH_TOKEN}:x-oauth-basic@github.com/${GITHUB_ORG_NAME}/${GITHUB_TEMPLATE_REPO_NAME} Template
export GITHUB_OAUTH_TOKEN=

pushd Submission > /dev/null
git checkout ${COMMIT_SHA} > /dev/null 2> /dev/null
popd > /dev/null

pushd Template > /dev/null
IFS=';' read -ra PATHS <<< "${PATHS_TO_COPY}"
for i in "${PATHS[@]}"; do
	if [[ $i == *".."* ]]
	then
		echo "Invalid path: $i";
		exit 1;
	fi
    cp -R --parents ./${i} ../Submission 
done
popd > /dev/null

cat << EOF > build.xml
<project name="${PROJECT_NAME}" default="help" basedir="Submission/${PROJECT_NAME}">
  <property name="proj" location="."/>
  <property name="src" location="src"/>
  <property name="build" location="build"/>
  <property name="jar" value="/usr/local/share/java" />

  <property name="ant.build.javac.target" value="1.8" />
  <property name="ant.build.javac.source" value="1.8" />

  <path id="build-classpath">
    <fileset dir="/usr/local/share/java">
        <include name="*.jar" />
    </fileset>
    <fileset dir=".">
        <include name="*.jar" />
    </fileset>
  </path>

  <path id="run-classpath">
    <path refid="build-classpath" />
    <path location="build" />
  </path>

  <target name="init">
    <tstamp/>
    <mkdir dir="build"/>
  </target>

  <target name="build" depends="init">
    <javac srcdir="src" destdir="build" includeantruntime="false" debug="true">
      <classpath refid="build-classpath" />
    </javac>
  </target>

  <target name="test" depends="build">
    <java classname="csc.projectrunner.JavaProjectRunner" fork="true">
      <classpath refid="run-classpath" />
      <arg value="${RESPONSE_FILE_PATH}" />
      <arg value="${TEST_CLASSES}" />
	  <jvmarg value="-Xms64m"/>
	  <jvmarg value="-Xmx256m"/>
    </java>
  </target>

  <target name="clean"  description="clean up" >
    <delete dir="build"/>
  </target>
</project>
EOF

ant test